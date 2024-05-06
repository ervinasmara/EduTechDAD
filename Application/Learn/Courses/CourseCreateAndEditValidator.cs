using FluentValidation;

namespace Application.Learn.Courses;
public class CourseCreateAndEditValidator : AbstractValidator<CourseCreateAndEditDto>
{
    public CourseCreateAndEditValidator()
    {
        RuleFor(x => x.CourseName).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.LessonName).NotEmpty();

        // Validasi untuk memastikan bahwa setidaknya satu dari LinkCourse diisi
        RuleFor(x => x.LinkCourse)
            .NotEmpty()
            .When(x => x.FileData == null) // Hanya memeriksa LinkCourse jika FileData kosong
            .WithMessage("LinkCourse must be provided if FileData is not provided.");
    }
}