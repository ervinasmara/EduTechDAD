using Application.Core;
using AutoMapper;
using Domain.Learn.Schedules;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Learn.Schedules
{
    public class Create
    {
        public class Command : IRequest<Result<ScheduleDto>>
        {
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
                // Cek apakah Lesson dengan LessonId yang diberikan ada di database
                var lesson = await _context.Lessons.FindAsync(request.ScheduleDto.LessonId);
                if (lesson == null)
                    return Result<ScheduleDto>.Failure("Lesson not found with the provided LessonId");

                // Cek apakah ClassRoom dengan ClassRoomId yang diberikan ada di database
                var classRoom = await _context.ClassRooms.FindAsync(request.ScheduleDto.ClassRoomId);
                if (classRoom == null)
                    return Result<ScheduleDto>.Failure("ClassRoom not found with the provided ClassRoomId");

                var schedule = new Schedule
                {
                    Day = request.ScheduleDto.Day,
                    StartTime = request.ScheduleDto.StartTime,
                    EndTime = request.ScheduleDto.EndTime,
                    LessonId = request.ScheduleDto.LessonId,
                    ClassRoomId = request.ScheduleDto.ClassRoomId
                };

                _context.Schedules.Add(schedule);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<ScheduleDto>.Failure("Failed to create Schedule");

                // Buat DTO dari jadwal yang telah dibuat
                var scheduleDto = _mapper.Map<ScheduleDto>(schedule);

                return Result<ScheduleDto>.Success(scheduleDto);
            }
        }
    }
}