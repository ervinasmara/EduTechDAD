using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
    public class ListScheduleById
    {
        public class Query : IRequest<Result<ScheduleGetDto>>
        {
            public Guid ScheduleId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ScheduleGetDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<ScheduleGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var schedule = await _context.Schedules
                        .Include(s => s.Lesson)
                            .ThenInclude(l => l.ClassRoom)
                        .Include(s => s.Lesson)
                            .ThenInclude(l => l.TeacherLessons)
                            .ThenInclude(tl => tl.Teacher)
                        .FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken);

                    if (schedule == null)
                        return Result<ScheduleGetDto>.Failure("Schedule not found");

                    var lessonName = schedule.Lesson?.LessonName;
                    var className = schedule.Lesson?.ClassRoom?.ClassName;
                    var nameTeacher = schedule.Lesson?.TeacherLessons?.FirstOrDefault()?.Teacher?.NameTeacher;

                    var scheduleDto = new ScheduleGetDto
                    {
                        Id = schedule.Id,
                        Day = schedule.Day,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        LessonName = lessonName,
                        ClassName = className,
                        NameTeacher = nameTeacher
                    };

                    return Result<ScheduleGetDto>.Success(scheduleDto);
                }
                catch (Exception ex)
                {
                    return Result<ScheduleGetDto>.Failure($"Failed to retrieve schedule: {ex.Message}");
                }
            }
        }
    }
}