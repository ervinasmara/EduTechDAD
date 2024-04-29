using Application.User.DTOs.Registration;
using FluentValidation;

namespace Application.User.Validation
{
    public class RegisterSuperadminValidator : AbstractValidator<RegisterSuperAdminDto>
    {
        public RegisterSuperadminValidator()
        {
            RuleFor(x => x.NameSuperAdmin).NotEmpty();
            RuleFor(x => x.Username).NotEmpty().Length(5, 20).WithMessage("Username length must be between 5 and 20 characters");
            RuleFor(x => x.Password)
                .NotEmpty()
                .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s).{8,16}")
                .WithMessage("Password must be complex");
        }
    }
}
