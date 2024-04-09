using FluentValidation;

namespace Application.Submission
{
    public class AssignmentSubmissionStudentValidator : AbstractValidator<AssignmentSubmissionStudentDto>
    {
        public AssignmentSubmissionStudentValidator()
        {
            RuleFor(x => x.Link)
                .NotEmpty()
                .When(x => x.FileData == null) // Hanya memeriksa Link jika FileData kosong
                .WithMessage("Link must be provided if FileData is not provided.");
        }
    }
}
