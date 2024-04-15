using Application.Attendances.DTOs;
using FluentValidation;

namespace Application.Attendances
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