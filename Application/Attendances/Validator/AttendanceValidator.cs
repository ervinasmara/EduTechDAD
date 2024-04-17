using Application.Attendances.DTOs;
using Application.Attendances.Validator;
using FluentValidation;

namespace Application.Attendances
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