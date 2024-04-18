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
                }
                else
                {
                    courseDto.UniqueNumberOfLesson = "UnknownUniqueNumberOfLesson";
                }

                // Get UniqueNumberOfClassRooms from CourseClassRooms
                courseDto.UniqueNumberOfClassRooms = course.CourseClassRooms.Select(ccr => ccr.ClassRoom.UniqueNumberOfClassRoom).ToList();

                return Result<CourseGetDto>.Success(courseDto);
            }
        }
    }
}
