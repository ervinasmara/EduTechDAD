using Application.Core;
using Domain.Announcement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Announcements.Query
{
    public class ListAnnouncement
    {
        public class Query : IRequest<Result<List<Announcement>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<Announcement>>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<Result<List<Announcement>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Mengembalikan hasil yang berhasil bersama dengan daftar Announcement dari basis data.
                return Result<List<Announcement>>.Success(await _context.Announcements.ToListAsync(cancellationToken));
            }
        }
    }
}