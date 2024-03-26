using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Domain.Present;

namespace Application.Presents
{
    public class Edit
    {
        public class Command : IRequest<Result<AttendanceDto>>
        {
            public Guid Id { get; set; }
            public AttendanceDto AttendanceDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
            {
                RuleFor(x => x.AttendanceDto).SetValidator(new AttendanceValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<AttendanceDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<AttendanceDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var attendance = await _context.Attendances.FindAsync(request.Id);

                // Periksa apakah attendance ditemukan
                if (attendance == null)
                {
                    return Result<AttendanceDto>.Failure("Presensi tidak ditemukan");
                }

                _mapper.Map(request.AttendanceDto, attendance);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<AttendanceDto>.Failure("Gagal untuk mengedit Presensi");
                }

                // Buat instance AttendanceDto yang mewakili hasil edit
                var editedAttendanceDto = _mapper.Map<AttendanceDto>(attendance);

                return Result<AttendanceDto>.Success(editedAttendanceDto);
            }
        }
    }
}
