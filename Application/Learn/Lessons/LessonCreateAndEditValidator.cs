using FluentValidation;

namespace Application.Learn.Lessons;
public class LessonCreateAndEditValidator : AbstractValidator<LessonCreateAndEditDto>
{
    public LessonCreateAndEditValidator()
    {
        RuleFor(x => x.LessonName).NotEmpty().WithMessage("Nama pelajaran tidak boleh kosong");
        RuleFor(x => x.ClassName).NotEmpty().WithMessage("Nama kelas tidak boleh kosong");
    }
}