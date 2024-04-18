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
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
            RuleFor(x => x.Password)
                .NotEmpty()
                .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
                .WithMessage("Password must be complex");
            RuleFor(x => x.Role).NotEmpty().WithMessage("Role is required.")
                                 .Equal(2).WithMessage("Role must be 2.");

            RuleFor(x => x.LessonName).NotEmpty().WithMessage("LessonName is required.");

            RuleFor(x => x.UniqueNumberOfClassRoom).NotEmpty().WithMessage("UniqueNumberOfClassRoom is required.");
        }
    }
}
