using Application.Core;
using Domain.Pengumuman;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Diagnostics;

namespace Application.Pengumumans
{
    public class List
    {
        public class Query : IRequest<Result<List<Pengumuman>>> // Dan ini akan berasal dari atau menggunakan IRequest dari Mediator
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<Pengumuman>>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context) // Memanggil DataContext dan menyebutnya sebagai context
            {
                _context = context;
            }
            public async Task<Result<List<Pengumuman>>> Handle(Query request, CancellationToken cancellationToken)
            {
                return Result<List<Pengumuman>>.Success(await _context.Pengumumans.ToListAsync(cancellationToken));
            }
        }
    }
}