using Application.Core;
using AutoMapper;
using Domain.Learn.Agenda;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Learn.Agenda
{
    public class Edit
    {
        public class Command : IRequest<Result<ScheduleDto>>
        {
            public Guid Id { get; set; }
            public ScheduleDto ScheduleDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ScheduleDto).SetValidator(new ScheduleValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<ScheduleDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ScheduleDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var schedule = await _context.Schedules.FindAsync(request.Id);

                if (schedule == null)
                    return Result<ScheduleDto>.Failure("Schedule not found");

                // Cek apakah Course dengan CourseId yang diberikan ada di database
                var course = await _context.Courses.FindAsync(request.ScheduleDto.CourseId);
                if (course == null)
                    return Result<ScheduleDto>.Failure("Course not found with the provided CourseId");

                // Cek apakah ClassRoom dengan ClassRoomId yang diberikan ada di database
                var classRoom = await _context.ClassRooms.FindAsync(request.ScheduleDto.ClassRoomId);
                if (classRoom == null)
                    return Result<ScheduleDto>.Failure("ClassRoom not found with the provided ClassRoomId");

                schedule.Day = request.ScheduleDto.Day;
                schedule.StartTime = request.ScheduleDto.StartTime;
                schedule.EndTime = request.ScheduleDto.EndTime;
                schedule.CourseId = request.ScheduleDto.CourseId;
                schedule.ClassRoomId = request.ScheduleDto.ClassRoomId;

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<ScheduleDto>.Failure("Failed to update Schedule");

                // Buat DTO dari jadwal yang telah diupdate
                var scheduleDto = _mapper.Map<ScheduleDto>(schedule);

                return Result<ScheduleDto>.Success(scheduleDto);
            }
        }
    }
}