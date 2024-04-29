using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query
{
    public class GetDetailsSubmissionBySubmissionIdAndStudentId
    {
        public class Query : IRequest<Result<SubmissionEditByStudentIdDto>>
        {
            public Guid SubmissionId { get; set; }
            public SubmissionEditByStudentIdDto SubmissionDto { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<SubmissionEditByStudentIdDto>>
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

            public async Task<Result<SubmissionEditByStudentIdDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var studentIdFromToken = Guid.Parse(_userAccessor.GetStudentIdFromToken());
                if (studentIdFromToken == Guid.Empty)
                    return Result<SubmissionEditByStudentIdDto>.Failure("Student ID not found in token.");

                var assignmentSubmission = await _context.AssignmentSubmissions
                    .Where(s => s.Id == request.SubmissionId && s.StudentId == studentIdFromToken)
                    .FirstOrDefaultAsync(cancellationToken);

                if (assignmentSubmission == null)
                    return Result<SubmissionEditByStudentIdDto>.Failure("No assignment submission found.");

                // Map the entity to the DTO
                var assignmentSubmissionDto = _mapper.Map<SubmissionEditByStudentIdDto>(assignmentSubmission);

                return Result<SubmissionEditByStudentIdDto>.Success(assignmentSubmissionDto);
            }
        }
    }
}
