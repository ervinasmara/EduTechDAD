using MediatR;
using Persistence;
using FluentValidation;
using Application.Core;
using AutoMapper;
using Domain.Attendances;
using Application.Attendances.DTOs;

namespace Application.Attendances
{
    public class Create
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
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AttendanceDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var classRoom = new Attendance
                {
                    Date = request.AttendanceDto.Date,
                    Status = request.AttendanceDto.Status,
                    StudentId = request.AttendanceDto.StudentId,
                };

                _context.Attendances.Add(classRoom);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<AttendanceDto>.Failure("Failed to create Attendance");

                // Buat DTO dari classRoom yang telah dibuat
                var classRoomDto = _mapper.Map<AttendanceDto>(classRoom);

                return Result<AttendanceDto>.Success(classRoomDto);
            }
        }
    }
}
