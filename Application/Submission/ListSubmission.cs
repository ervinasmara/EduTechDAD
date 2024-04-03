using Application.Core;
using AutoMapper;
using Domain.Submission;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission
{
    public class ListSubmission
    {
        public class Query : IRequest<Result<List<AssignmentSubmissionGetDto>>>
        {
            /* Kita tidak memerlukan parameter tambahan untuk meneruskan ke query*/
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentSubmissionGetDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AssignmentSubmissionGetDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var submissions = await _context.AssignmentSubmissions.ToListAsync(cancellationToken);
                var submissionDtos = _mapper.Map<List<AssignmentSubmissionGetDto>>(submissions);

                return Result<List<AssignmentSubmissionGetDto>>.Success(submissionDtos);
            }
        }
    }
}