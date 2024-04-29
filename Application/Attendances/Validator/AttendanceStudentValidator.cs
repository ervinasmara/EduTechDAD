using FluentValidation;

namespace Application.Attendances.Validator
{
    public class AttendanceStudentValidator : AbstractValidator<AttendanceStudentCreateDto>
    {
        public AttendanceStudentValidator()
        {
            RuleFor(x => x.StudentId).NotEmpty();
            RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required.");
            RuleFor(x => x.Status).InclusiveBetween(1, 3).WithMessage("Status must be between 1 and 3.");
        }
    }
}
