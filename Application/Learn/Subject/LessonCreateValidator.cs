using FluentValidation;

namespace Application.Learn.Subject
{
    public class LessonCreateValidator : AbstractValidator<LessonCreateDto>
    {
        public LessonCreateValidator()
        {
            RuleFor(x => x.LessonName).NotEmpty();
        }
    }
}
