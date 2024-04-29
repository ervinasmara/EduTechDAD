using Application.Core;
using Application.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query
{
    public class GetSubmissionForStudentBySubmissionId
    {
        public class Query : IRequest<Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>>
        {
            public Guid SubmissionId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
            {
                _context = context;
                _userAccessor = userAccessor;
                _mapper = mapper;
            }

            public async Task<Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>> Handle(Query request, CancellationToken cancellationToken)
            {
                var studentIdFromToken = Guid.Parse(_userAccessor.GetStudentIdFromToken());
                if (studentIdFromToken == Guid.Empty)
                    return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure("Student ID not found in token.");

                var assignmentSubmissionDto = await _context.AssignmentSubmissions
                    .Where(s => s.Id == request.SubmissionId && s.StudentId == studentIdFromToken)
                    .ProjectTo<AssignmentSubmissionGetByAssignmentIdAndStudentId>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(cancellationToken);

                if (assignmentSubmissionDto == null)
                    return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure("No assignment submission found.");

                return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Success(assignmentSubmissionDto);
            }
        }
    }
}