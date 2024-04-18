using Application.Attendances.DTOs;
using FluentValidation;

namespace Application.Attendances.Validator
{
    public class AttendanceValidator : AbstractValidator<AttendanceDto>
    {
        public AttendanceValidator()
        {
            RuleFor(x => x.Date).NotEmpty();
            RuleForEach(x => x.AttendanceStudentCreate)
                .SetValidator(new AttendanceStudentValidator());
        }
    }
}