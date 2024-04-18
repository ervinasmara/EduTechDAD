using Application.Core;
using Application.Interface;
using Application.Learn.Courses;
using Application.Learn.Lessons;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ClassRooms
{
    public class ClassRoomTeacher
    {
        public class Query : IRequest<Result<object>>
        {
            // Tidak memerlukan properti karena hanya mengambil informasi dari token
        }

        public class QueryHandler : IRequestHandler<Query, Result<object>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public QueryHandler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<object>> Handle(Query request, CancellationToken cancellationToken)
            {
                var teacherId = _userAccessor.GetTeacherIdFromToken();

                if (teacherId == null)
                {
                    return Result<object>.Failure("Teacher ID not found in token");
                }

                var teacherLessons = await _context.TeacherLessons
                    .Where(tl => tl.TeacherId == Guid.Parse(teacherId))
                    .Select(tl => tl.Lesson)
                    .ToListAsync();

                var teacherClassRooms = await _context.TeacherClassRooms
                    .Include(tcr => tcr.ClassRoom)
                    .Where(tcr => tcr.TeacherId == Guid.Parse(teacherId))
                    .Select(tcr => tcr.ClassRoom)
                    .ToListAsync();

                var lessonDtos = teacherLessons.Select(lesson =>
                    new
                    {
                        Id = lesson.Id,
                        LessonName = lesson.LessonName,
                        UniqueNumberOfLesson = lesson.UniqueNumberOfLesson,
                    }).ToList();

                var classRoomDtos = teacherClassRooms.Select(classRoom =>
                    new
                    {
                        ClassName = classRoom.ClassName,
                        UniqueNumberOfClassRoom = classRoom.UniqueNumberOfClassRoom,
                    }).ToList();

                return Result<object>.Success(new
                {
                    teacherId = teacherId,
                    lessons = lessonDtos,
                    classrooms = classRoomDtos
                });
            }
        }
    }
}