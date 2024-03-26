using Application.Core;
using Domain.Present;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Presents
{
    public class List
    {
        public class Query : IRequest<Result<List<Attendance>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<Attendance>>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<Result<List<Attendance>>> Handle(Query request, CancellationToken cancellationToken)
            {
                return Result<List<Attendance>>.Success(await _context.Attendances.ToListAsync(cancellationToken));
            }
        }
    }
}