using Application.User.DTOs.Registration;
using FluentValidation;

namespace Application.User.Validation
{
    public class RegisterStudentValidator : AbstractValidator<RegisterStudentDto>
    {
        public RegisterStudentValidator()
        {
            RuleFor(x => x.NameStudent).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.BirthDate).NotEmpty().WithMessage("BirthDate is required");
            RuleFor(x => x.BirthPlace).NotEmpty().WithMessage("BirthPlace is required");
            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches("^[0-9]{8,13}$").WithMessage("Phone number must be between 8 and 13 digits and contain only numbers.");
            RuleFor(x => x.ParentName).NotEmpty().WithMessage("ParentName is required");
            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(gender => gender >= 1 && gender <= 2)
                .WithMessage("Invalid Gender value. Use 1 for Male, 2 for Female");
            RuleFor(x => x.Role).Equal(3).WithMessage("Role must be 3");
            RuleFor(x => x.UniqueNumberOfClassRoom).NotEmpty().WithMessage("UniqueNumberOfClassRoom is required");
        }
    }
}
