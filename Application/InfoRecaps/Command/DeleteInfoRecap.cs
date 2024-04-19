using Application.Core;
using MediatR;
using Persistence;

namespace Application.InfoRecaps.Command
{
    public class DeleteInfoRecap
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
                // Menggunakan FindAsync untuk mencari InfoRecap berdasarkan ID yang diberikan dalam permintaan.
                var infoRecap = await _context.InfoRecaps.FindAsync(request.Id);

                // Jika InfoRecap tidak ditemukan, kembalikan null.
                if (infoRecap == null) return null;

                // Menghapus InfoRecap dari konteks.
                _context.Remove(infoRecap);

                // Menyimpan perubahan ke basis data dan memeriksa apakah perubahan berhasil disimpan.
                var result = await _context.SaveChangesAsync() > 0;

                // Jika gagal menyimpan perubahan, kembalikan pesan kesalahan.
                if (!result) return Result<Unit>.Failure("Failed to delete InfoRecap");

                // Jika berhasil, kembalikan hasil yang berhasil tanpa nilai.
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}