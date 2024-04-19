using Application.Core;
using Domain.InfoRecaps;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.InfoRecaps
{
    public class ListInfoRecap
    {
        public class Query : IRequest<Result<List<InfoRecap>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<InfoRecap>>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<Result<List<InfoRecap>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Mengembalikan hasil yang berhasil bersama dengan daftar InfoRecap dari basis data.
                return Result<List<InfoRecap>>.Success(await _context.InfoRecaps.ToListAsync(cancellationToken));
            }
        }
    }
}