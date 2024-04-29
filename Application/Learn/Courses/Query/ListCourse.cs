using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses.Query
{
    public class ListCourse
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
                    .OrderByDescending(c => c.CreatedAt)
                    .ProjectTo<CourseGetDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                if (!courses.Any())
                {
                    return Result<List<CourseGetDto>>.Failure("No courses found.");
                }

                return Result<List<CourseGetDto>>.Success(courses);
            }
        }
    }
}