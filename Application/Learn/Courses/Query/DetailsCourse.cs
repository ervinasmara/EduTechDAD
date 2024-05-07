using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Courses.Query;
public class DetailsCourse
{
    public class Query : IRequest<Result<CourseGetDto>>
    {
        public Guid CourseId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<CourseGetDto>>
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
            var courses = await _context.Courses
                .Where(a => a.Id == request.CourseId && a.Status != 0)
                .OrderByDescending(c => c.CreatedAt)
                .ProjectTo<CourseGetDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (courses == null)
            {
                return Result<CourseGetDto>.Failure("Materi tidak ditemukan");
            }

            return Result<CourseGetDto>.Success(courses);
        }
    }
}