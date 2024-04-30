using FluentValidation;

namespace Application.Submission.Validator
{
    public class SubmissionGradesForTeacherValidator : AbstractValidator<AssignmentSubmissionTeacherDto>
    {
        public SubmissionGradesForTeacherValidator()
        {
            RuleFor(x => x.Grade)
                  .NotEmpty()
                  .InclusiveBetween(0.0f, 100.0f)
                  .WithMessage("Grade must be a number between 0 and 100");
            RuleFor(x => x.Comment).NotEmpty();
        }
    }
}