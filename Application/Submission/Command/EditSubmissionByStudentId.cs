using MediatR;
using Application.Core;
using Application.Interface;
using FluentValidation;

namespace Application.Submission.Command
{
    public class EditSubmissionByStudentId
    {
        public class Command : IRequest<Result<SubmissionEditByStudentIdDto>>
        {
            public Guid SubmissionId { get; set; }
            public SubmissionEditByStudentIdDto SubmissionDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.SubmissionDto.Link)
                .NotEmpty()
                .When(x => x.SubmissionDto.FileData == null) // Hanya memeriksa AssignmentLink jika FileData kosong
                .WithMessage("AssignmentLink must be provided if FileData is not provided.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<SubmissionEditByStudentIdDto>>
        {
            private readonly ISubmissionService _submissionService;

            public Handler(ISubmissionService submissionService)
            {
                _submissionService = submissionService;
            }

            public async Task<Result<SubmissionEditByStudentIdDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var createResult = await _submissionService.EditSubmissionByStudentIdAsync(request.SubmissionDto, request.SubmissionId, cancellationToken);

                if (!createResult.IsSuccess)
                    return Result<SubmissionEditByStudentIdDto>.Failure(createResult.Error);

                return Result<SubmissionEditByStudentIdDto>.Success(request.SubmissionDto);
            }
        }
    }
}