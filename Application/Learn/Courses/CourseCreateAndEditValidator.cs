using FluentValidation;

namespace Application.Learn.Courses;
public class CourseCreateAndEditValidator : AbstractValidator<CourseCreateAndEditDto>
{
    public CourseCreateAndEditValidator()
    {
        RuleFor(x => x.CourseName).NotEmpty().WithMessage("Nama materi tidak boleh kosong");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Deskripsi tidak boleh kosong");
        RuleFor(x => x.LessonName).NotEmpty().WithMessage("Nama mapel tidak boleh kosong");

        // Validasi untuk memastikan bahwa setidaknya satu dari LinkCourse diisi
        RuleFor(x => x.LinkCourse)
            .NotEmpty()
            .When(x => x.FileData == null) // Hanya memeriksa LinkCourse jika FileData kosong
            .WithMessage("Link materi harus disediakan jika File tidak disediakan.");
    }
}