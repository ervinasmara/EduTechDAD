using MediatR;
using Application.Core;
using Application.Interface;
using Application.Assignments;
using FluentValidation;

namespace Application.Submission.Command
{
    public class CreateSubmissionByStudentId
    {
        public class Command : IRequest<Result<SubmissionCreateByStudentIdDto>>
        {
            public SubmissionCreateByStudentIdDto SubmissionDto { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.SubmissionDto).SetValidator(new SubmissionCreateByStudentIdValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<SubmissionCreateByStudentIdDto>>
        {
            private readonly ISubmissionService _submissionService;

            public Handler(ISubmissionService submissionService)
            {
                _submissionService = submissionService;
            }

            public async Task<Result<SubmissionCreateByStudentIdDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var createResult = await _submissionService.CreateSubmissionAsync(request.SubmissionDto, cancellationToken);

                if (!createResult.IsSuccess)
                    return Result<SubmissionCreateByStudentIdDto>.Failure(createResult.Error);

                return Result<SubmissionCreateByStudentIdDto>.Success(request.SubmissionDto);
            }
        }
    }
}