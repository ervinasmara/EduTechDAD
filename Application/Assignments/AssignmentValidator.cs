using FluentValidation;

namespace Application.Assignments;
public class AssignmentValidator : AbstractValidator<AssignmentCreateAndEditDto>
{
    public AssignmentValidator()
    {
        RuleFor(x => x.AssignmentName).NotEmpty().WithMessage("Nama tugas tidak boleh kosong");
        RuleFor(x => x.AssignmentDate).NotEmpty().WithMessage("Tanggal tugas tidak boleh kosong");
        RuleFor(x => x.AssignmentDeadline).NotEmpty().WithMessage("Tenggat waktu tugas tidak boleh kosong");
        RuleFor(x => x.AssignmentDescription).NotEmpty().WithMessage("Deskripsi tugas tidak boleh kosong");
        RuleFor(x => x.CourseId).NotEmpty().WithMessage("Materi tugas tidak boleh kosong");
        RuleFor(x => x.TypeOfSubmission)
            .NotEmpty().WithMessage("Tipe pengumpulan tidak boleh kosong")
            .Must(x => x == 1 || x == 2)
            .WithMessage("Jenis pengiriman harus berupa 1 (untuk file) atau 2 (untuk tautan).");

        RuleFor(x => x.AssignmentLink)
            .NotEmpty()
            .When(x => x.AssignmentFileData == null) // Hanya memeriksa AssignmentLink jika FileData kosong
            .WithMessage("Link Tugas harus disediakan jika FileData tidak disediakan.");
    }
}