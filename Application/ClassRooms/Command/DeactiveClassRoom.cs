using Application.Core;
using MediatR;
using Persistence;

namespace Application.ClassRooms.Command;
public class DeactiveClassRoom
{
    public class Command : IRequest<Result<object>>
    {
        public Guid ClassRoomId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<object>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari Ruang Kelas Berdasarkan ID **/
            var classRoom = await _context.ClassRooms.FindAsync(request.ClassRoomId);

            /** Langkah 2: Memeriksa Ketersediaan Ruang Kelas **/
            if (classRoom == null) return null; // Jika tidak ada ruang kelas, tidak perlu dilakukan deaktivasi

            /** Langkah 3: Memperbarui Status Ruang Kelas Menjadi 0 **/
            classRoom.Status = 0; // Update status menjadi 0

            /** Langkah 4: Menyimpan Perubahan ke Database **/
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            /** Langkah 5: Memeriksa Hasil Simpan **/
            if (!result) return Result<object>.Failure("Gagal menonaktifkan ClassRoom");

            /** Langkah 6: Mengembalikan Hasil dalam Bentuk Success Result dengan Pesan **/
            return Result<object>.Success(new { Message = "Status ClassRoom berhasil diperbarui" });
        }
    }
}