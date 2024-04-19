using Application.Core;
using Domain.Announcement;
using MediatR;
using Persistence;

namespace Application.Announcements.Query
{
    public class DetailsAnnouncement
    {
        public class Query : IRequest<Result<Announcement>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Announcement>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Announcement>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Menggunakan FindAsync untuk mencari Announcement berdasarkan ID yang diberikan dalam permintaan.
                var announcement = await _context.Announcements.FindAsync(request.Id);

                // Mengembalikan hasil yang berhasil bersama dengan entitas Announcement yang ditemukan.
                return Result<Announcement>.Success(announcement);
            }
        }
    }
}