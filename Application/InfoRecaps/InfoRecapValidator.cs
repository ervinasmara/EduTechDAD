using FluentValidation;

namespace Application.InfoRecaps
{
    public class InfoRecapValidator : AbstractValidator<InfoRecapCreateDto>
    {
        public InfoRecapValidator()
        {
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
        }
    }
}