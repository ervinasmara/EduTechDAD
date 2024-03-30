using Domain.Submission;
using FluentValidation;

namespace Application.Submission
{
    public class AssignmentSubmissionValidator : AbstractValidator<AssignmentSubmissionStatusDto>
    {
        public AssignmentSubmissionValidator()
        {
            RuleFor(x => x.AssignmentId).NotEmpty();
            RuleFor(x => x.StudentId).NotEmpty();
        }
    }
}
