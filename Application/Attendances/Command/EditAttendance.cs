using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;

namespace Application.Attendances.Command;
public class EditAttendance
{
    public class Command : IRequest<Result<AttendanceEditDto>>
    {
        public Guid Id { get; set; }
        public AttendanceEditDto AttendanceEditDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<AttendanceEditDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<AttendanceEditDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari kehadiran berdasarkan ID **/
            var attendance = await _context.Attendances.FindAsync(request.Id);

            /** Langkah 2: Memeriksa kehadiran **/
            // Periksa apakah attendance ditemukan
            if (attendance == null)
            {
                return Result<AttendanceEditDto>.Failure("Kehadiran tidak ditemukan");
            }

            /** Langkah 3: Memetakan data edit ke kehadiran yang ditemukan **/
            _mapper.Map(request.AttendanceEditDto, attendance);

            /** Langkah 4: Menyimpan perubahan ke database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<AttendanceEditDto>.Failure("Gagal untuk edit Kehadiran");
            }

            /** Langkah 5: Membuat instance AttendanceEditDto untuk hasil edit **/
            var editedAttendanceEditDto = _mapper.Map<AttendanceEditDto>(attendance);

            return Result<AttendanceEditDto>.Success(editedAttendanceEditDto);
        }
    }
}

public class CommandValidatorDto : AbstractValidator<AttendanceEditDto>
{
    public CommandValidatorDto()
    {
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status tidak boleh kosong");
        RuleFor(x => x.Status).InclusiveBetween(1, 3).WithMessage("Status harus antara 1 dan 3");
    }
}