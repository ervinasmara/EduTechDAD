using Application.Core;
using MediatR;
using Persistence;

namespace Application.User.Teachers.Command
{
    public class DeactivateTeacher
    {
        public class Command : IRequest<Result<object>>
        {
            public Guid TeacherId { get; set; }
        }

        public class EditTeacherStatusHandler : IRequestHandler<Command, Result<object>>
        {
            private readonly DataContext _context;

            public EditTeacherStatusHandler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
            {
                /** Langkah 1: Mencari guru berdasarkan ID **/
                var teacher = await _context.Teachers.FindAsync(request.TeacherId);

                /** Langkah 2: Memeriksa apakah guru ditemukan **/
                if (teacher == null)
                    return Result<object>.Failure("Guru tidak ditemukan");

                /** Langkah 3: Mengubah status guru menjadi nonaktif **/
                teacher.Status = 0;

                /** Langkah 4: Menyimpan perubahan ke database **/
                await _context.SaveChangesAsync(cancellationToken);

                /** Langkah 5: Mengembalikan hasil dalam bentuk Success Result dengan pesan **/
                return Result<object>.Success(new { Message = "Status guru berhasil diperbarui" });
            }
        }
    }
}
