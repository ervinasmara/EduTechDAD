using Application.Core;
using Application.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ClassRooms.Query
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
                    .Select(tl => tl.LessonId)
                    .ToListAsync();

                var classRooms = await _context.ClassRooms
                    .Where(classRoom => classRoom.Lessons.Any(lesson => teacherLessons.Contains(lesson.Id)))
                    .Select(classRoom => new
                    {
                        classRoom.ClassName,
                        classRoom.LongClassName,
                        classRoom.UniqueNumberOfClassRoom,
                    })
                    .ToListAsync();

                return Result<object>.Success(new
                {
                    teacherId,
                    classrooms = classRooms
                });
            }
        }
    }
}