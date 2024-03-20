using Domain.Pengumuman;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Pengumumans
{
    public class List
    {
        public class Query : IRequest<List<Pengumuman>> // Dan ini akan berasal dari atau menggunakan IRequest dari Mediator
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, List<Pengumuman>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context) // Memanggil DataContext dan menyebutnya sebagai context
            {
                _context = context;
            }
            public async Task<List<Pengumuman>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Pengumumans.ToListAsync();
            }
        }
    }
}