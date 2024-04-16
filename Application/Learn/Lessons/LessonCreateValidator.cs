using FluentValidation;

namespace Application.Learn.Lessons
{
    public class LessonCreateValidator : AbstractValidator<LessonCreateDto>
    {
        public LessonCreateValidator()
        {
            RuleFor(x => x.LessonName).NotEmpty();
            RuleFor(x => x.NameTeacher).NotEmpty();
        }
    }
}
