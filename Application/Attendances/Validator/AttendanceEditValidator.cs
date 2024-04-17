using Application.Attendances.DTOs;
using FluentValidation;

namespace Application.Attendances.Validator
{
    public class AttendanceEditValidator : AbstractValidator<AttendanceEditDto>
    {
        public AttendanceEditValidator()
        {
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required.");
            RuleFor(x => x.Status).InclusiveBetween(1, 3).WithMessage("Status must be between 1 and 3.");
        }
    }
}