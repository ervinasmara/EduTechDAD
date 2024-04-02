using Application.Core;
using AutoMapper;
using Domain.Submission;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission
{
    public class GetAssignmentSubmissionByClassRoomAndAssignment
    {
        public class Query : IRequest<Result<AssignmentSubmissionGetByIdCRandA>>
        {
            public Guid ClassRoomId { get; set; }
            public Guid AssignmentId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentSubmissionGetByIdCRandA>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentSubmissionGetByIdCRandA>> Handle(Query request, CancellationToken cancellationToken)
            {
                var assignmentSubmission = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == request.AssignmentId && s.Student.ClassRoomId == request.ClassRoomId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (assignmentSubmission == null)
                    return Result<AssignmentSubmissionGetByIdCRandA>.Failure("Assignment submission not found.");

                var assignmentSubmissionDto = _mapper.Map<AssignmentSubmissionGetByIdCRandA>(assignmentSubmission);

                return Result<AssignmentSubmissionGetByIdCRandA>.Success(assignmentSubmissionDto);
            }
        }
    }
}