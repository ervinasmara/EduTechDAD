using Application.Core;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses
{
    public class Details
    {
        public class Query : IRequest<Result<CourseGetDto>> // Mengubah DTO menjadi CourseGetDto
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<CourseGetDto>> // Mengubah Course menjadi CourseGetDto
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<CourseGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var course = await _context.Courses
                    .Include(c => c.Lesson)
                    .Include(c => c.CourseClassRooms)
                    .ThenInclude(ccr => ccr.ClassRoom)
                    .FirstOrDefaultAsync(c => c.Id == request.Id);

                if (course == null)
                    return Result<CourseGetDto>.Failure("Course not found.");

                var courseDto = _mapper.Map<CourseGetDto>(course);

                // Set FileName based on CourseName and FileData extension
                if (!string.IsNullOrEmpty(course.CourseName) && course.FileData != null)
                {
                    courseDto.FileName = $"{course.CourseName}.{GetFileExtension.FileExtensionHelper(course.FileData)}";
                }
                else
                {
                    courseDto.FileName = "No File";
                }

                // Set UniqueNumberOfLesson from Lesson entity
                if (course.Lesson != null)
                {
                    courseDto.UniqueNumberOfLesson = course.Lesson.UniqueNumberOfLesson;
                    courseDto.LessonName = course.Lesson.LessonName; // Menambahkan lessonname
                }
                else
                {
                    courseDto.UniqueNumberOfLesson = "UnknownUniqueNumberOfLesson";
                }


                // Get UniqueNumberOfClassRooms from CourseClassRooms
                courseDto.UniqueNumberOfClassRooms = course.CourseClassRooms.Select(ccr => ccr.ClassRoom.UniqueNumberOfClassRoom).ToList();

                // Get Teacher Name
                var teacherCourse = await _context.TeacherCourses.FirstOrDefaultAsync(tc => tc.CourseId == course.Id, cancellationToken);
                if (teacherCourse != null)
                {
                    var teacherId = teacherCourse.TeacherId;
                    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);
                    if (teacher != null)
                    {
                        courseDto.NameTeacher = teacher.NameTeacher;
                    }
                }

                return Result<CourseGetDto>.Success(courseDto);
            }
        }
    }
}
