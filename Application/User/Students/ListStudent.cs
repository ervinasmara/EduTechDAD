using Application.Core;
using Application.Learn.Lessons;
using Application.User.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User.Students
{
    public class ListStudent
    {
        public class Query : IRequest<Result<List<StudentGetAllDto>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<StudentGetAllDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<StudentGetAllDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var students = await _context.Students
                    .Where(s => s.Status != 0) // Filter status tidak sama dengan 0
                    .OrderBy(s => s.Nis) // Urutkan berdasarkan NIS secara naik
                    .ProjectTo<StudentGetAllDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return Result<List<StudentGetAllDto>>.Success(students);
            }
        }
    }
}