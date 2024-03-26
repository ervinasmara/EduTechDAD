using Domain.Learn.Study;
using FluentValidation;

namespace Application.Learn.Study
{
    public class CourseValidator : AbstractValidator<CourseDto>
    {
        public CourseValidator()
        {
            RuleFor(x => x.CourseName).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.FileData).NotEmpty();
            RuleFor(x => x.UniqueNumber)
                .NotEmpty()
                .Matches(@"^\d{2}$") // Memastikan panjang string adalah 3 digit
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
