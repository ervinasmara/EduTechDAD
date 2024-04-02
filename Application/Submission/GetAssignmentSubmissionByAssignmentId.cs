//using Application.Core;
//using AutoMapper;
//using Domain.Submission;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Persistence;

//namespace Application.Submission
//{
//    public class GetAssignmentSubmissionByAssignmentId
//    {
//        public class Query : IRequest<Result<List<AssignmentSubmissionGetByIdAssignmentDto>>>
//        {
//            public Guid AssignmentId { get; set; }
//            public AssignmentSubmissionGetByIdAssignmentDto AssignmentSubmissionGetByIdAssignmentDto { get; set; }
//        }

//        public class Handler : IRequestHandler<Query, Result<List<AssignmentSubmissionGetByIdAssignmentDto>>>
//        {
//            private readonly DataContext _context;
//            private readonly IMapper _mapper;

//            public Handler(DataContext context, IMapper mapper)
//            {
//                _context = context;
//                _mapper = mapper;
//            }

//            public async Task<Result<List<AssignmentSubmissionGetByIdAssignmentDto>>> Handle(Query request, CancellationToken cancellationToken)
//            {
//                var assignmentSubmissions = await _context.AssignmentSubmissions
//                    .Where(x => x.AssignmentId == request.AssignmentId)
//                    .ToListAsync();

//                if (assignmentSubmissions == null || !assignmentSubmissions.Any())
//                    return Result<List<AssignmentSubmissionGetByIdAssignmentDto>>.Failure($"No AssignmentSubmissions found for Assignment ID {request.AssignmentId}");

//                var assignmentSubmissionDtos = _mapper.Map<List<AssignmentSubmissionGetByIdAssignmentDto>>(assignmentSubmissions);

//                return Result<List<AssignmentSubmissionGetByIdAssignmentDto>>.Success(assignmentSubmissionDtos);
//            }
//        }
//    }
//}