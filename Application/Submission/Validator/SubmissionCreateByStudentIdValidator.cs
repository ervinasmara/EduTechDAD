﻿using FluentValidation;

namespace Application.Submission.Validator
{
    public class SubmissionCreateByStudentIdValidator : AbstractValidator<SubmissionCreateByStudentIdDto>
    {
        public SubmissionCreateByStudentIdValidator()
        {
            RuleFor(x => x.AssignmentId).NotEmpty();
            RuleFor(x => x.Link)
                .NotEmpty()
                .When(x => x.FileData == null) // Hanya memeriksa Link jika FileData kosong
                .WithMessage("AssignmentLink must be provided if FileData is not provided.");
        }
    }
}