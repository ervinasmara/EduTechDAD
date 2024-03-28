using Application.Core;
using Domain.Learn.Subject;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Subject
{
    public class List
    {
        public class Query : IRequest<Result<List<Lesson>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<Lesson>>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<Result<List<Lesson>>> Handle(Query request, CancellationToken cancellationToken)
            {
                return Result<List<Lesson>>.Success(await _context.Lessons.ToListAsync(cancellationToken));
            }
        }
    }
}