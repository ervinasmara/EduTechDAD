using Application.Core;
using AutoMapper;
using Domain.Submission;
using MediatR;
using Persistence;

namespace Application.Submission
{
    public class DetailsSubmission
    {
        public class Query : IRequest<Result<AssignmentSubmissionGetDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentSubmissionGetDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentSubmissionGetDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var submission = await _context.AssignmentSubmissions.FindAsync(request.Id);

                if (submission == null)
                {
                    return Result<AssignmentSubmissionGetDto>.Failure("Submission not found.");
                }

                var submissionDto = _mapper.Map<AssignmentSubmissionGetDto>(submission);

                return Result<AssignmentSubmissionGetDto>.Success(submissionDto);
            }
        }
    }
}