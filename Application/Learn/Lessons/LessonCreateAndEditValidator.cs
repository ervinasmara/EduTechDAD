using FluentValidation;

namespace Application.Learn.Lessons
{
    public class LessonCreateAndEditValidator : AbstractValidator<LessonCreateAndEditDto>
    {
        public LessonCreateAndEditValidator()
        {
            RuleFor(x => x.LessonName).NotEmpty();
            RuleFor(x => x.ClassName).NotEmpty();
        }
    }
}
