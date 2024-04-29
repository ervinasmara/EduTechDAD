using Application.User.DTOs.Registration;
using FluentValidation;

namespace Application.User.Validation
{
    public class RegisterAdminValidator : AbstractValidator<RegisterAdminDto>
    {
        public RegisterAdminValidator()
        {
            RuleFor(x => x.NameAdmin).NotEmpty();
            RuleFor(x => x.Username).NotEmpty().Length(5, 20).WithMessage("Username length must be between 5 and 20 characters");
            RuleFor(x => x.Password)
                .NotEmpty()
                .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
                .WithMessage("Password must be complex");
        }
    }
}
