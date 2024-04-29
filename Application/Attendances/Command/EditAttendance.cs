using MediatR;
using Persistence;
using AutoMapper;
using FluentValidation;
using Application.Core;
using Application.Attendances.Validator;

namespace Application.Attendances.Command
{
    public class EditAttendance
    {
        public class Command : IRequest<Result<AttendanceEditDto>>
        {
            public Guid Id { get; set; }
            public AttendanceEditDto AttendanceEditDto { get; set; }
        }

        public class CommandValidatorDto : AbstractValidator<Command>
        {
            public CommandValidatorDto()
            {
                RuleFor(x => x.AttendanceEditDto).SetValidator(new AttendanceEditValidator());
            }
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
                var attendance = await _context.Attendances.FindAsync(request.Id);

                // Periksa apakah attendance ditemukan
                if (attendance == null)
                {
                    return Result<AttendanceEditDto>.Failure("Attendance has not found");
                }

                _mapper.Map(request.AttendanceEditDto, attendance);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<AttendanceEditDto>.Failure("Failed to edit Attendance");
                }

                // Buat instance AttendanceEditDto yang mewakili hasil edit
                var editedAttendanceEditDto = _mapper.Map<AttendanceEditDto>(attendance);

                return Result<AttendanceEditDto>.Success(editedAttendanceEditDto);
            }
        }
    }
}
