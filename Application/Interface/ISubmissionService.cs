using Application.Core;
using Application.Submission;
using Domain.Submission;

namespace Application.Interface;
public interface ISubmissionService
{
    Task<Result<AssignmentSubmission>> CreateSubmissionAsync(SubmissionCreateByStudentIdDto submissionDto, CancellationToken cancellationToken);
}