using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query
{
    public class GetListSubmissionForSuperAdmin
    {
        public class Query : IRequest<Result<List<AssignmentSubmissionListGradeDto>>>
        {
            // Tidak memerlukan parameter tambahan untuk meneruskan ke query
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentSubmissionListGradeDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AssignmentSubmissionListGradeDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var submission = await _context.AssignmentSubmissions
                    .OrderByDescending(c => c.SubmissionTime)
                    .ProjectTo<AssignmentSubmissionListGradeDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                if (!submission.Any())
                {
                    return Result<List<AssignmentSubmissionListGradeDto>>.Failure("No submission found.");
                }

                return Result<List<AssignmentSubmissionListGradeDto>>.Success(submission);
            }
        }
    }
}
