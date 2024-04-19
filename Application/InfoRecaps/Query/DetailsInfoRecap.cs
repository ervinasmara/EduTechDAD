using Application.Core;
using Domain.InfoRecaps;
using MediatR;
using Persistence;

namespace Application.InfoRecaps
{
    public class DetailsInfoRecap
    {
        public class Query : IRequest<Result<InfoRecap>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<InfoRecap>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<InfoRecap>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Menggunakan FindAsync untuk mencari InfoRecap berdasarkan ID yang diberikan dalam permintaan.
                var announcement = await _context.InfoRecaps.FindAsync(request.Id);

                // Mengembalikan hasil yang berhasil bersama dengan entitas InfoRecap yang ditemukan.
                return Result<InfoRecap>.Success(announcement);
            }
        }
    }
}