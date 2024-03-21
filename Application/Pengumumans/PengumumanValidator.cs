using Domain.Pengumuman;
using FluentValidation;

namespace Application.Pengumumans
{
    public class PengumumanValidator : AbstractValidator<Pengumuman>
    {
        public PengumumanValidator()
        {
            RuleFor(x => x.Deskripsi).NotEmpty();
            RuleFor(x => x.Tanggal).NotEmpty();
        }
    }
}