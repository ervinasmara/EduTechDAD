using Application.Core;
using Application.Interface;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Submission.Query
{
    public class GetSubmissionForTeacherBySubmissionId
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
                var teacherIdFromToken = Guid.Parse(_userAccessor.GetTeacherIdFromToken());
                if (teacherIdFromToken == Guid.Empty)
                    return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure("Teacher ID not found in token.");

                var assignmentSubmission = await _context.AssignmentSubmissions
                    .Include(s => s.Assignment)
                        .ThenInclude(a => a.Course)
                            .ThenInclude(c => c.Lesson)
                                .ThenInclude(l => l.TeacherLessons)
                    .Where(s => s.Id == request.SubmissionId)
                    .SingleOrDefaultAsync(cancellationToken);

                if (assignmentSubmission == null)
                    return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure("No assignment submission found.");

                // Check if the teacher is related to the lesson
                var isTeacherRelatedToLesson = assignmentSubmission.Assignment.Course.Lesson.TeacherLessons
                    .Any(tl => tl.TeacherId == teacherIdFromToken);

                if (!isTeacherRelatedToLesson)
                    return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Failure("The submission is not related to the teacher.");

                var assignmentSubmissionDto = _mapper.Map<AssignmentSubmissionGetByAssignmentIdAndStudentId>(assignmentSubmission);

                return Result<AssignmentSubmissionGetByAssignmentIdAndStudentId>.Success(assignmentSubmissionDto);
            }
        }
    }
}