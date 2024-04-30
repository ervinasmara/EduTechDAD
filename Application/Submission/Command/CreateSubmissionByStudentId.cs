using MediatR;
using Application.Core;
using Application.Interface;
using FluentValidation;
using Application.Submission.Validator;

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
                /** Langkah 1: Memanggil layanan untuk membuat pengajuan **/
                var createResult = await _submissionService.CreateSubmissionAsync(request.SubmissionDto, cancellationToken);

                /** Langkah 2: Memeriksa apakah pembuatan pengajuan berhasil **/
                if (!createResult.IsSuccess)
                    return Result<SubmissionCreateByStudentIdDto>.Failure(createResult.Error);

                /** Langkah 3: Mengembalikan hasil yang berhasil **/
                return Result<SubmissionCreateByStudentIdDto>.Success(request.SubmissionDto);
            }
        }
    }
}