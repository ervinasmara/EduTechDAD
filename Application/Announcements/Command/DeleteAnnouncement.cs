using Application.Core;
using MediatR;
using Persistence;

namespace Application.Announcements.Command
{
    public class DeleteAnnouncement
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Mencari Announcement berdasarkan ID yang diberikan dalam permintaan.
                var announcement = await _context.Announcements.FindAsync(request.Id);

                // Jika Announcement tidak ditemukan, kembalikan null.
                if (announcement == null) return null;

                // Menghapus Announcement dari konteks.
                _context.Remove(announcement);

                // Menyimpan perubahan ke basis data dan memeriksa apakah perubahan berhasil disimpan.
                var result = await _context.SaveChangesAsync() > 0;

                // Jika gagal menyimpan perubahan, kembalikan pesan kesalahan.
                if (!result) return Result<Unit>.Failure("Failed to delete Announcement");

                // Jika berhasil, kembalikan hasil yang berhasil tanpa nilai.
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}