using Application.Core;
using AutoMapper;
using Domain.Learn.Schedules;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Command
{
    public class CreateSchedule
    {
        public class Command : IRequest<Result<ScheduleCreateAndEditDto>>
        {
            public ScheduleCreateAndEditDto ScheduleCreateAndEditDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ScheduleCreateAndEditDto).SetValidator(new ScheduleValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<ScheduleCreateAndEditDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<ScheduleCreateAndEditDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Temukan lesson berdasarkan LessonName yang diberikan
                    var lesson = await _context.Lessons
                        .FirstOrDefaultAsync(l => l.LessonName == request.ScheduleCreateAndEditDto.LessonName, cancellationToken);

                    if (lesson == null)
                        return Result<ScheduleCreateAndEditDto>.Failure($"Lesson with name '{request.ScheduleCreateAndEditDto.LessonName}' not found.");

                    var schedule = new Schedule
                    {
                        Day = request.ScheduleCreateAndEditDto.Day,
                        StartTime = request.ScheduleCreateAndEditDto.StartTime,
                        EndTime = request.ScheduleCreateAndEditDto.EndTime,
                        LessonId = lesson.Id
                    };

                    _context.Schedules.Add(schedule);

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                        return Result<ScheduleCreateAndEditDto>.Failure("Failed to create Schedule");

                    var scheduleDto = _mapper.Map<ScheduleCreateAndEditDto>(schedule);
                    scheduleDto.LessonName = lesson.LessonName; // Set LessonName in response

                    return Result<ScheduleCreateAndEditDto>.Success(scheduleDto);
                }
                catch (Exception ex)
                {
                    return Result<ScheduleCreateAndEditDto>.Failure($"Failed to create schedule: {ex.Message}");
                }
            }
        }
    }
}