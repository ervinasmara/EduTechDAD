using Domain.Learn.Subject;
using FluentValidation;

namespace Application.Learn.Subject
{
    public class LessonValidator : AbstractValidator<LessonDto>
    {
        public LessonValidator()
        {
            RuleFor(x => x.LessonName).NotEmpty();
            RuleFor(x => x.UniqueNumberOfLesson)
                .NotEmpty()
                .Matches(@"^\d{2}$") // Memastikan panjang string adalah 2 digit
                .WithMessage("UniqueNumber must be 2 digits")
                .Must(BeInRange)
                .WithMessage("UniqueNumber must be in the range 01 to 99");
        }

        private bool BeInRange(string uniqueNumber)
        {
            if (int.TryParse(uniqueNumber, out int number))
            {
                return number >= 1 && number <= 99;
            }
            return false;
        }
    }
}
