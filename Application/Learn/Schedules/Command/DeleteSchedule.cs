using Application.Core;
using MediatR;
using Persistence;

namespace Application.Learn.Schedules.Command;
public class DeleteSchedule
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid ScheduleId { get; set; }
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
            /** Langkah 1: Mencari Jadwal Berdasarkan ID **/
            var schedule = await _context.Schedules.FindAsync(request.ScheduleId);

            /** Langkah 2: Memeriksa Ketersediaan Jadwal **/
            if (schedule == null) return null; // Jika tidak ada jadwal, tidak ada yang perlu dihapus

            /** Langkah 3: Menghapus Jadwal dari Database **/
            _context.Remove(schedule);

            /** Langkah 4: Menyimpan Perubahan ke Database **/
            var result = await _context.SaveChangesAsync() > 0;

            /** Langkah 5: Memeriksa Hasil Penghapusan **/
            if (!result) return Result<Unit>.Failure("Gagal untuk menghapus jadwal");

            /** Langkah 6: Mengembalikan Hasil dalam Bentuk Success Result **/
            return Result<Unit>.Success(Unit.Value);
        }
    }
}