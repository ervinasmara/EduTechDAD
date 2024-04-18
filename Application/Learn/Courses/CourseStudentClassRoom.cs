using Application.Core;
using Application.Interface;
using Application.Learn.GetFileName;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses
{
    public class CourseStudentClassRoom
    {
        public class Query : IRequest<Result<object>>
        {
        }

        public class QueryHandler : IRequestHandler<Query, Result<object>>
        {
            private readonly IUserAccessor _userAccessor;
            private readonly DataContext _context;

            public QueryHandler(IUserAccessor userAccessor, DataContext context)
            {
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Result<object>> Handle(Query request, CancellationToken cancellationToken)
            {
                var classroomId = _userAccessor.GetClassRoomIdFromToken();

                if (classroomId == null)
                {
                    return Result<object>.Failure("Classroom ID not found in token");
                }

                var classroom = await _context.ClassRooms
                    .Include(c => c.CourseClassRooms)
                        .ThenInclude(ccr => ccr.Course)
                            .ThenInclude(course => course.Lesson)
                    .FirstOrDefaultAsync(c => c.Id == Guid.Parse(classroomId), cancellationToken);

                if (classroom == null)
                {
                    return Result<object>.Failure("Classroom not found");
                }

                var coursesInClassroom = await _context.Courses
                    .Include(c => c.Lesson)
                    .Where(c => c.CourseClassRooms.Any(ccr => ccr.ClassRoomId == classroom.Id))
                    .ToListAsync(cancellationToken);

                var courseDtos = coursesInClassroom.Select(course => new CourseGetDto
                {
                    Id = course.Id,
                    CourseName = course.CourseName,
                    Description = course.Description,
                    FileName = course.FileData != null ? $"{course.CourseName}.{GetFileExtension.FileExtensionHelper(course.FileData)}" : "No File",
                    FileData = course.FileData,
                    LinkCourse = course.LinkCourse,
                    UniqueNumberOfLesson = course.Lesson != null ? course.Lesson.UniqueNumberOfLesson : "UnknownUniqueNumberOfLesson",
                    UniqueNumberOfClassRooms = course.CourseClassRooms?.Select(ccr => ccr.ClassRoom.UniqueNumberOfClassRoom).ToList() ?? new List<string>(),
                    NameTeacher = GetTeacherName(course.Id), // GetTeacherName di bawah
                    LessonName = course.Lesson?.LessonName
                }).ToList();

                return Result<object>.Success(new
                {
                    classroomId = classroom.Id,
                    className = classroom.ClassName,
                    courses = courseDtos
                });
            }

            private string GetTeacherName(Guid courseId)
            {
                var teacherCourse = _context.TeacherCourses
                    .Include(tc => tc.Teacher)
                    .FirstOrDefault(tc => tc.CourseId == courseId);

                return teacherCourse?.Teacher?.NameTeacher;
            }
        }
    }
}
