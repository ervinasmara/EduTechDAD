using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Application.Attendances.Command;
public class CreateAttendance
{
    public class Command : IRequest<Result<AttendanceCreateDto>>
    {
        public Guid ClassRoomId { get; set; }
        public AttendanceCreateDto AttendanceCreateDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<AttendanceCreateDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<AttendanceCreateDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validasi ClassRoomId
            var classroom = await _context.ClassRooms.FindAsync(request.ClassRoomId);
            if (classroom == null)
                return Result<AttendanceCreateDto>.Failure($"Invalid ClassRoomId: {request.ClassRoomId}");

            // Dapatkan daftar siswa dalam kelas
            var studentsInClassroom = await _context.Students
                .Where(s => s.ClassRoomId == request.ClassRoomId && s.Status != 0)
                .ToListAsync(cancellationToken);

            // Periksa apakah semua siswa diabsen
            var studentsNotYetAttendance = studentsInClassroom
                .Where(s => !request.AttendanceCreateDto.AttendanceStudentCreate
                    .Any(a => a.StudentId == s.Id))
                .ToList();

            if (studentsNotYetAttendance.Any())
            {
                var studentNames = string.Join(", ", studentsNotYetAttendance.Select(s => s.NameStudent));
                return Result<AttendanceCreateDto>.Failure($"Students not yet attended: {studentNames}");
            }

            /** Langkah 1: Validasi duplikasi siswa **/
            var duplicateStudentIds = request.AttendanceCreateDto.AttendanceStudentCreate
                .GroupBy(s => s.StudentId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateStudentIds.Any())
                return Result<AttendanceCreateDto>.Failure($"Duplicate StudentIds: {string.Join(", ", duplicateStudentIds)}");

            /** Langkah 2: Validasi StudentId yang valid **/
            var validStudentIds = request.AttendanceCreateDto.AttendanceStudentCreate
                .Select(s => s.StudentId)
                .ToList();

            var invalidStudentIds = validStudentIds.Except(_context.Students.Select(s => s.Id)).ToList();

            if (invalidStudentIds.Any())
                return Result<AttendanceCreateDto>.Failure($"Invalid StudentIds: {string.Join(", ", invalidStudentIds)}");

            /** Langkah 3: Validasi tidak ada kehadiran yang ada pada tanggal sama **/
            var existingAttendances = await _context.Attendances
                .Where(a => a.Date == request.AttendanceCreateDto.Date && validStudentIds.Contains(a.StudentId))
                .ToListAsync(cancellationToken);

            if (existingAttendances.Any())
            {
                var existingStudentIds = existingAttendances.Select(a => a.StudentId).Distinct().ToList();
                return Result<AttendanceCreateDto>.Failure($"Attendance already exists for StudentIds: {string.Join(", ", existingStudentIds)} on {request.AttendanceCreateDto.Date}");
            }

            /** Langkah 4: Menambahkan entri kehadiran **/
            var attendances = request.AttendanceCreateDto.AttendanceStudentCreate
                .Select(_mapper.Map<Attendance>)
                .ToList();

            foreach (var attendance in attendances)
            {
                /** Langkah 4.1: Mengatur tanggal yang sama **/
                attendance.Date = request.AttendanceCreateDto.Date;
                _context.Attendances.Add(attendance);
            }

            /** Langkah 5: Menyimpan perubahan ke database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                return Result<AttendanceCreateDto>.Failure("Failed to create Attendance");

            /** Langkah 6: Memetakan kembali ke Dto **/
            var attendanceDto = _mapper.Map<AttendanceCreateDto>(request.AttendanceCreateDto);
            attendanceDto.AttendanceStudentCreate = attendances.Select(a => _mapper.Map<AttendanceStudentCreateDto>(a)).ToList();

            return Result<AttendanceCreateDto>.Success(attendanceDto);
        }
    }
}

public class CommandValidator : AbstractValidator<AttendanceCreateDto>
{
    public CommandValidator()
    {
        RuleFor(x => x.Date).NotEmpty();
        RuleForEach(x => x.AttendanceStudentCreate)
            .SetValidator(new AttendanceStudentValidator());
    }
}

public class AttendanceStudentValidator : AbstractValidator<AttendanceStudentCreateDto>
{
    public AttendanceStudentValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required.");
        RuleFor(x => x.Status).InclusiveBetween(1, 3).WithMessage("Status must be between 1 and 3.");
    }
}