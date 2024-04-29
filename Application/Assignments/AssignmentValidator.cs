﻿using FluentValidation;

namespace Application.Assignments
{
    public class AssignmentValidator : AbstractValidator<AssignmentCreateAndEditDto>
    {
        public AssignmentValidator()
        {
            RuleFor(x => x.AssignmentName).NotEmpty();
            RuleFor(x => x.AssignmentDate).NotEmpty();
            RuleFor(x => x.AssignmentDeadline).NotEmpty();
            RuleFor(x => x.AssignmentDescription).NotEmpty();
            RuleFor(x => x.CourseId).NotEmpty();

            RuleFor(x => x.AssignmentLink)
                .NotEmpty()
                .When(x => x.AssignmentFileData == null) // Hanya memeriksa AssignmentLink jika FileData kosong
                .WithMessage("AssignmentLink must be provided if FileData is not provided.");
        }
    }
}