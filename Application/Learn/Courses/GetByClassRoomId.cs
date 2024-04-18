using Application.Core;
using Application.Learn.GetFileName;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses
{
    public class GetByClassRoomId
    {
        public class Query : IRequest<Result<List<CourseGetDto>>>
        {
            public Guid ClassRoomId { get; set; }
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
                    .Include(c => c.CourseClassRooms)
                    .ThenInclude(ccr => ccr.ClassRoom)
                    .Where(c => c.CourseClassRooms.Any(ccr => ccr.ClassRoomId == request.ClassRoomId))
                    .ToListAsync(cancellationToken);

                var courseDtos = new List<CourseGetDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<CourseGetDto>(course);
                    courseDto.UniqueNumberOfLesson = course.Lesson.UniqueNumberOfLesson;

                    if (course.FileData != null)
                    {
                        courseDto.FileName = $"{course.CourseName}.{GetFileExtension.FileExtensionHelper(course.FileData)}";
                    }
                    else
                    {
                        courseDto.FileName = "No File";
                    }

                    courseDto.UniqueNumberOfClassRooms = course.CourseClassRooms.Select(ccr => ccr.ClassRoom.UniqueNumberOfClassRoom).ToList();
                    courseDtos.Add(courseDto);
                }

                return Result<List<CourseGetDto>>.Success(courseDtos);
            }
        }
    }
}