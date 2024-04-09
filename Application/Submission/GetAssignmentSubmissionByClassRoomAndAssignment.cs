using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission
{
    public class GetAssignmentSubmissionByClassRoomAndAssignment
    {
        public class Query : IRequest<Result<List<AssignmentSubmissionGetByIdCRandA>>>
        {
            public Guid ClassRoomId { get; set; }
            public Guid AssignmentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<AssignmentSubmissionGetByIdCRandA>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AssignmentSubmissionGetByIdCRandA>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var assignmentSubmissions = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == request.AssignmentId && s.Student.ClassRoomId == request.ClassRoomId)
                    .ToListAsync(cancellationToken);

                if (assignmentSubmissions == null || assignmentSubmissions.Count == 0)
                    return Result<List<AssignmentSubmissionGetByIdCRandA>>.Failure("No assignment submissions found.");

                var assignmentSubmissionDtos = _mapper.Map<List<AssignmentSubmissionGetByIdCRandA>>(assignmentSubmissions);

                return Result<List<AssignmentSubmissionGetByIdCRandA>>.Success(assignmentSubmissionDtos);
            }
        }
    }
}