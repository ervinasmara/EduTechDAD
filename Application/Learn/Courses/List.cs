using Application.Core;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses
{
    public class List
    {
        public class Query : IRequest<Result<List<CourseGetDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<CourseGetDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<CourseGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var courses = await _context.Courses
                    .Include(c => c.Lesson)
                    .Include(c => c.CourseClassRooms) // Include CourseClassRooms
                    .ThenInclude(ccr => ccr.ClassRoom) // Include ClassRoom for each CourseClassRoom
                    .ToListAsync(cancellationToken);

                var courseDtos = new List<CourseGetDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<CourseGetDto>(course);
                    courseDto.UniqueNumberOfLesson = course.Lesson.UniqueNumberOfLesson; // Set UniqueNumberOfLesson from Lesson
                    if (course.FileData != null)
                    {
                        courseDto.FileName = $"{course.CourseName}.{GetFileExtension.FileExtensionHelper(course.FileData)}";
                    }
                    else
                    {
                        courseDto.FileName = "No File";
                    }

                    // Get UniqueNumberOfClassRooms from CourseClassRooms
                    courseDto.UniqueNumberOfClassRooms = course.CourseClassRooms.Select(ccr => ccr.ClassRoom.UniqueNumberOfClassRoom).ToList();

                    courseDtos.Add(courseDto);
                }

                return Result<List<CourseGetDto>>.Success(courseDtos);
            }
        }
    }
}