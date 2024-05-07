using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.User.Admins;
public class DeactivateAdmin
{
    public class Command : IRequest<Result<object>>
    {
        public Guid AdminId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AdminId).NotEmpty();
        }
    }

    public class EditAdminStatusHandler : IRequestHandler<Command, Result<object>>
    {
        private readonly DataContext _context;

        public EditAdminStatusHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
        {
            /** Langkah 1: Mencari Admin Berdasarkan ID **/
            var admin = await _context.Admins.FindAsync(request.AdminId);

            /** Langkah 2: Memeriksa Ketersediaan Admin **/
            if (admin == null)
                return Result<object>.Failure("Admin tidak ditemukan");

            /** Langkah 3: Mengubah Status Admin Menjadi Nonaktif **/
            admin.Status = 0;

            /** Langkah 4: Menyimpan Perubahan ke Database **/
            await _context.SaveChangesAsync(cancellationToken);

            /** Langkah 5: Mengembalikan Hasil dalam Bentuk Success Result dengan Pesan **/
            return Result<object>.Success(new { Message = "Status admin berhasil diperbarui" });
        }
    }
}