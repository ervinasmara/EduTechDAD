using Application.Core;
using AutoMapper;
using Domain.Learn.Study;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Study
{
    public class List
    {
        public class Query : IRequest<Result<List<CourseGetAllDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<CourseGetAllDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<CourseGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var courses = await _context.Courses.Include(c => c.Lesson).ToListAsync(cancellationToken);
                var courseDtos = new List<CourseGetAllDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<CourseGetAllDto>(course);
                    courseDto.UniqueNumber = course.Lesson.UniqueNumber; // Set UniqueNumber dari Lesson
                    courseDtos.Add(courseDto);
                }

                return Result<List<CourseGetAllDto>>.Success(courseDtos);
            }
        }
    }
}