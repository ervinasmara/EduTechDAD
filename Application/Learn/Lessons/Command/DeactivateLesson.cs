using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Learn.Lessons.Command;
public class DeactivateLesson
{
    public class Command : IRequest<Result<object>>
    {
        public Guid LessonId { get; set; }
    }

    public class EditLessonStatusHandler : IRequestHandler<Command, Result<object>>
    {
        private readonly DataContext _context;

        public EditLessonStatusHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Ubah status Lesson menjadi 0 **/
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(c => c.Id == request.LessonId, cancellationToken);

            // Periksa apakah ada lesson yang ditemukan
            if (lesson == null)
            {
                return Result<object>.Failure("Lesson tidak ditemukan");
            }

            /** Langkah 2: Mengubah status lesson menjadi 0 (nonaktif) **/
            lesson.Status = 0;

            /** Langkah 3: Simpan perubahan status ke database **/
            await _context.SaveChangesAsync(cancellationToken);

            /** Langkah 4: Mengembalikan hasil berhasil dengan pesan **/
            return Result<object>.Success(new { Message = "Status pelajaran berhasil diperbarui" });
        }
    }
}