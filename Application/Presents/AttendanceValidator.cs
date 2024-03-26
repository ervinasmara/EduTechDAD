using Domain.Present;
using FluentValidation;

namespace Application.Presents
{
    public class AttendanceValidator : AbstractValidator<AttendanceDto>
    {
        public AttendanceValidator()
        {
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
            RuleFor(x => x.StudentId).NotEmpty();
        }
    }
}