using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Application.Attendances.Validator;

namespace Application.Attendances.Command
{
    public class CreateAttendance
    {
        public class Command : IRequest<Result<AttendanceDto>>
        {
            public AttendanceDto AttendanceDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AttendanceDto).SetValidator(new AttendanceValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<AttendanceDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<AttendanceDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validasi: memastikan data siswa unik
                var duplicateStudentIds = request.AttendanceDto.AttendanceStudentCreate
                    .GroupBy(s => s.StudentId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateStudentIds.Any())
                    return Result<AttendanceDto>.Failure($"Duplicate StudentIds: {string.Join(", ", duplicateStudentIds)}");

                // Validasi: memastikan studentId yang disediakan ada di database
                var validStudentIds = request.AttendanceDto.AttendanceStudentCreate
                    .Select(s => s.StudentId)
                    .ToList();

                var invalidStudentIds = validStudentIds.Except(_context.Students.Select(s => s.Id)).ToList();

                if (invalidStudentIds.Any())
                    return Result<AttendanceDto>.Failure($"Invalid StudentIds: {string.Join(", ", invalidStudentIds)}");

                // Validasi: memastikan tidak ada entri kehadiran untuk studentId tertentu pada tanggal yang sama
                var existingAttendances = await _context.Attendances
                    .Where(a => a.Date == request.AttendanceDto.Date && validStudentIds.Contains(a.StudentId))
                    .ToListAsync(cancellationToken);

                if (existingAttendances.Any())
                {
                    var existingStudentIds = existingAttendances.Select(a => a.StudentId).Distinct().ToList();
                    return Result<AttendanceDto>.Failure($"Attendance already exists for StudentIds: {string.Join(", ", existingStudentIds)} on {request.AttendanceDto.Date}");
                }

                // Tambahkan entri kehadiran untuk setiap siswa
                foreach (var studentAttendanceDto in request.AttendanceDto.AttendanceStudentCreate)
                {
                    var studentAttendance = new Attendance
                    {
                        Status = studentAttendanceDto.Status,
                        StudentId = studentAttendanceDto.StudentId,
                        Date = request.AttendanceDto.Date, // Set tanggal yang sama untuk setiap entri kehadiran
                    };

                    _context.Attendances.Add(studentAttendance);
                }

                // Simpan perubahan ke database
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<AttendanceDto>.Failure("Failed to create Attendance");

                var attendanceDto = new AttendanceDto
                {
                    Date = request.AttendanceDto.Date,
                    AttendanceStudentCreate = request.AttendanceDto.AttendanceStudentCreate.ToList()
                };

                return Result<AttendanceDto>.Success(attendanceDto);
            }
        }
    }
}