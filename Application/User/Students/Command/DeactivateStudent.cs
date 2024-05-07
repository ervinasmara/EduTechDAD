using Application.Core;
using MediatR;
using Persistence;

namespace Application.User.Students.Command;
public class DeactivateStudent
{
    public class Command : IRequest<Result<object>>
    {
        public Guid StudentId { get; set; }
    }
    public class EditStudentStatusHandler : IRequestHandler<Command, Result<object>>
    {
        private readonly DataContext _context;

        public EditStudentStatusHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari siswa berdasarkan ID **/
            var student = await _context.Students.FindAsync(request.StudentId);

            /** Langkah 2: Memeriksa apakah siswa ditemukan **/
            if (student == null)
                return Result<object>.Failure("Siswa tidak ditemukan");

            // Mengubah status Student menjadi 0
            /** Langkah 3: Mengubah status siswa menjadi nonaktif **/
            student.Status = 0;

            /** Langkah 4: Menyimpan perubahan ke database **/
            await _context.SaveChangesAsync(cancellationToken);

            /** Langkah 5: Mengembalikan hasil dalam bentuk Success Result dengan pesan **/
            return Result<object>.Success(new { Message = "Status siswa berhasil diperbarui" });
        }
    }
}