using Application.User.DTOs.Registration;
using FluentValidation;

namespace Application.User.Validation
{
    public class RegisterTeacherValidator : AbstractValidator<RegisterTeacherDto>
    {
        public RegisterTeacherValidator()
        {
            RuleFor(x => x.NameTeacher).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.BirthDate).NotEmpty().WithMessage("BirthDate is required.");
            RuleFor(x => x.BirthPlace).NotEmpty().WithMessage("BirthPlace is required.");
            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.")
                                          .Matches("^[0-9]{8,13}$").WithMessage("Phone number must be between 8 and 13 digits and contain only numbers.");
            RuleFor(x => x.Nip).NotEmpty().WithMessage("Nip is required.");
            RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.")
                                  .InclusiveBetween(1, 2).WithMessage("Gender must be 1 for Male or 2 for Female.");
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
            RuleFor(x => x.Password)
                .NotEmpty()
                .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
                .WithMessage("Password must be complex");

            RuleFor(x => x.LessonNames).NotEmpty().WithMessage("LessonNames is required.");
        }
    }
}
