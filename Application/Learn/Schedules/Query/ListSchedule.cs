using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
    public class ListSchedule
    {
        public class Query : IRequest<Result<List<ScheduleGetDto>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<ScheduleGetDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var schedules = await _context.Schedules
                        .Include(s => s.Lesson)
                            .ThenInclude(l => l.ClassRoom)
                        .Include(s => s.Lesson)
                            .ThenInclude(l => l.TeacherLessons)
                            .ThenInclude(tl => tl.Teacher)
                        .OrderBy(s => s.Day) // Urutkan berdasarkan Day
                        .ToListAsync(cancellationToken);

                    var scheduleDtos = schedules.Select(schedule =>
                    {
                        var lessonName = schedule.Lesson?.LessonName;
                        var className = schedule.Lesson?.ClassRoom?.ClassName;
                        var nameTeacher = schedule.Lesson?.TeacherLessons?.FirstOrDefault()?.Teacher?.NameTeacher;

                        return new ScheduleGetDto
                        {
                            Id = schedule.Id,
                            Day = schedule.Day,
                            StartTime = schedule.StartTime,
                            EndTime = schedule.EndTime,
                            LessonName = lessonName,
                            ClassName = className,
                            NameTeacher = nameTeacher
                        };
                    }).ToList();

                    return Result<List<ScheduleGetDto>>.Success(scheduleDtos);
                }
                catch (Exception ex)
                {
                    return Result<List<ScheduleGetDto>>.Failure($"Failed to retrieve schedules: {ex.Message}");
                }
            }
        }
    }
}