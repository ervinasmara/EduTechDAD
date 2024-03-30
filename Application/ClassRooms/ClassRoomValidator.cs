using Domain.Class;
using FluentValidation;

namespace Application.ClassRooms
{
    public class ClassRoomValidator : AbstractValidator<ClassRoomDto>
    {
        public ClassRoomValidator()
        {
            RuleFor(x => x.ClassName).NotEmpty();
            RuleFor(x => x.UniqueNumberOfClassRoom)
                .NotEmpty()
                .Matches(@"^\d{3}$") // Memastikan panjang string adalah 3 digit
                .WithMessage("UniqueNumber must be 3 digits")
                .Must(BeInRange)
                .WithMessage("UniqueNumber must be in the range 001 to 100");
        }

        private bool BeInRange(string uniqueNumber)
        {
            if (int.TryParse(uniqueNumber, out int number))
            {
                return number >= 1 && number <= 100;
            }
            return false;
        }
    }
}
