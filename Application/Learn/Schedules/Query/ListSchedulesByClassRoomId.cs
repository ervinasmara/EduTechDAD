using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Schedules.Query
{
    public class ListSchedulesByClassRoomId
    {
        public class Query : IRequest<Result<List<ScheduleGetDto>>>
        {
            // Tidak memerlukan properti karena hanya mengambil informasi dari token
        }

        public class Handler : IRequestHandler<Query, Result<List<ScheduleGetDto>>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<ScheduleGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var classRoomId = _userAccessor.GetClassRoomIdFromToken();

                    if (string.IsNullOrEmpty(classRoomId))
                        return Result<List<ScheduleGetDto>>.Failure("Classroom ID not found in token");

                    var schedules = await _context.Schedules
                        .Include(s => s.Lesson)
                            .ThenInclude(l => l.ClassRoom)
                        .Include(s => s.Lesson)
                            .ThenInclude(l => l.TeacherLessons)
                            .ThenInclude(tl => tl.Teacher)
                        .Where(s => s.Lesson.ClassRoomId.ToString() == classRoomId)
                        .OrderBy(s => s.Day)
                        .ToListAsync(cancellationToken);

                    if (!schedules.Any())
                        return Result<List<ScheduleGetDto>>.Failure("Schedules not found for this classroom.");

                    var scheduleDtos = schedules.Select(schedule => new ScheduleGetDto
                    {
                        Id = schedule.Id,
                        Day = schedule.Day,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        LessonName = schedule.Lesson?.LessonName,
                        ClassName = schedule.Lesson?.ClassRoom?.ClassName,
                        NameTeacher = schedule.Lesson?.TeacherLessons?.FirstOrDefault()?.Teacher?.NameTeacher
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
