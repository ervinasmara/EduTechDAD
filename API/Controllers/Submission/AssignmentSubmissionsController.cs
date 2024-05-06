using Application.Submission;
using Application.Submission.Command;
using Application.Submission.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Submission;
public class AssignmentSubmissionsController : BaseApiController
{
    /** Get Submission For Student By AssignmentId **/
    [Authorize(Policy = "RequireRole3")]
    [HttpGet("getSubmissionForStudentByAssignmentId/{id}")]
    public async Task<ActionResult> GetAssignmentSubmissionById(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new GetSubmissionForStudentByAssignmentId.Query { AssignmentId = id }, ct));
    }

    /** Get List Submission For Teacher Grades By LessonId And AssignmentId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpGet("GetListSubmissionForTeacherGrades")]
    public async Task<ActionResult> GetListSubmissionForTeacherGrades(Guid LessonId, Guid AssignmentId, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new GetListSubmissionForTeacherGrades.Query { LessonId = LessonId, AssignmentId = AssignmentId }, ct));
    }

    /** Get Submission For Teacher By SubmissionId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpGet("getSubmissionForTeacherBySubmissionId/{id}")]
    public async Task<ActionResult> GetSubmissionForTeacherBySubmissionId(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new GetSubmissionForTeacherBySubmissionId.Query { SubmissionId = id }, ct));
    }

    /** Download Submission By SubmissionId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadSubmission(Guid id)
    {
        var result = await Mediator.Send(new DownloadSubmission.Query { SubmissionId = id });

        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }

        var downloadFileDto = result.Value;
        if (downloadFileDto.FileData == null || downloadFileDto.ContentType == null)
        {
            return NotFound("File data or content type is null.");
        }

        return File(downloadFileDto.FileData, downloadFileDto.ContentType, downloadFileDto.FileName);
    }

    /** Create Submission By StudentId **/
    [Authorize(Policy = "RequireRole3")]
    [HttpPost]
    public async Task<IActionResult> CreateSubmissionByStudentId(SubmissionCreateByStudentIdDto assignmentSubmissionStatusDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateSubmissionByStudentId.Command { SubmissionDto = assignmentSubmissionStatusDto }, ct));
    }

    /** Edit Submission By TeacherId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpPut("teacher/{id}")]
    public async Task<IActionResult> EditSubmissionByTeacherId(Guid id, AssignmentSubmissionTeacherDto announcementSubmissionTeacherDto, CancellationToken ct)
    {
        var result = await Mediator.Send(new EditSubmissionByTeacherId.Command { SubmissionId = id, AssignmentSubmissionTeacherDto = announcementSubmissionTeacherDto }, ct);

        return HandleResult(result);
    }
}