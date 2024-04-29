using Application.Submission;
using Application.Submission.Command;
using Application.Submission.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Submission
{
    public class AssignmentSubmissionsController : BaseApiController
    {
        [Authorize(Policy = "RequireRole4")]
        [HttpGet]
        public async Task<IActionResult> GetAssignmentSubmissionsForSuperAdmin(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetListSubmissionForSuperAdmin.Query(), ct));
        }

        //[Authorize(Policy = "RequireRole2")]
        //[HttpGet("submissionparam")]
        //public async Task<ActionResult> GetSubmissionParam([FromQuery] string ClassName, [FromQuery] string AssignmentName, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new ListSubmissionByTeacherId.Query(ClassName, AssignmentName), ct));
        //}

        //[Authorize(Policy = "RequireRole2")]
        //[HttpGet("submissiondetails")]
        //public async Task<ActionResult> GetSubmissionDetails([FromQuery] string className, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new GetAssignmentDescriptionByClassName.Query { ClassName = className }, ct));
        //}

        [Authorize(Policy = "RequireRole3")]
        [HttpGet("getSubmissionForStudentByAssignmentId/{id}")]
        public async Task<ActionResult> GetAssignmentSubmissionById(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetSubmissionForStudentByAssignmentId.Query { AssignmentId = id }, ct));
        }

        [Authorize(Policy = "RequireRole3")]
        [HttpGet("getSubmissionForStudentBySubmissionId/{id}")]
        public async Task<ActionResult> GetSubmissionForStudentBySubmissionId(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetSubmissionForStudentBySubmissionId.Query { SubmissionId = id }, ct));
        }

        [Authorize(Policy = "RequireRole2")]
        [HttpGet("GetListSubmissionForTeacherGrades")]
        public async Task<ActionResult> GetListSubmissionForTeacherGrades(Guid LessonId, Guid AssignmentId, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetListSubmissionForTeacherGrades.Query { LessonId = LessonId, AssignmentId = AssignmentId }, ct));
        }

        [Authorize(Policy = "RequireRole2")]
        [HttpGet("getSubmissionForTeacherBySubmissionId/{id}")]
        public async Task<ActionResult> GetSubmissionForTeacherBySubmissionId(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetSubmissionForTeacherBySubmissionId.Query { SubmissionId = id }, ct));
        }

        //[Authorize(Policy = "RequireRole2")]
        //[HttpGet("teacher/{teacherId}")]
        //public async Task<ActionResult> GetAssingmentSubmissionByTeacherId(Guid teacherId, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new GetSubmissionByTeacherId.Query { TeacherId = teacherId }, ct));
        //}

        [Authorize(Policy = "RequireRole3")]
        [HttpPost]
        public async Task<IActionResult> CreateAssignmentSubmissionCoba(SubmissionCreateByStudentIdDto assignmentSubmissionStatusDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateSubmissionByStudentId.Command { SubmissionDto = assignmentSubmissionStatusDto }, ct));
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpPut("teacher/{id}")]
        public async Task<IActionResult> EditAssignmentSubmissionTeachert(Guid id, AssignmentSubmissionTeacherDto announcementSubmissionTeacherDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditSubmissionByTeacherId.Command { SubmissionId = id, AssignmentSubmissionTeacherDto = announcementSubmissionTeacherDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole3")]
        [HttpPut("student/{id}")]
        public async Task<IActionResult> EditAssignmentSubmissionStudent(Guid id, SubmissionEditByStudentIdDto announcementSubmissionStudentDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditSubmissionByStudentId.Command { SubmissionId = id, SubmissionDto = announcementSubmissionStudentDto }, ct);

            return HandleResult(result);
        }

        //[Authorize(Policy = "RequireRole3OrRole4")]
        //[HttpPut("student/{studentId}/assignment/{assignmentId}")]
        //public async Task<IActionResult> EditAssignmentSubmissionStudent(Guid studentId, Guid assignmentId, AssignmentSubmissionStudentDto announcementSubmissionStudentDto, CancellationToken ct)
        //{
        //    var result = await Mediator.Send(new UpdateStudent2Id.Command { StudentId = studentId, AssignmentId = assignmentId, AssignmentSubmissionStudentDto = announcementSubmissionStudentDto }, ct);

        //    return HandleResult(result);
        //}

        //[HttpPut("1/{id}")]
        //public async Task<IActionResult> EditAssignmentSubmission(Guid id, AssignmentSubmissionStudentDto assignmentSubmissionStudentDto, CancellationToken ct)
        //{
        //    var result = await Mediator.Send(new UpdateStudent1.Command { Id = id, AssignmentSubmissionStudentDto = assignmentSubmissionStudentDto }, ct);

        //    if (result.IsSuccess)
        //    {
        //        // Jika berhasil, kirimkan data yang telah diubah
        //        return Ok(result.Value); // Atau Ok(result.Value) jika ingin mengirimkan status 200 OK
        //    }
        //    else
        //    {
        //        // Jika gagal, kirimkan pesan kesalahan
        //        return BadRequest(result.Error); // Atau NotFound(result.Error) atau lainnya sesuai dengan kebutuhan
        //    }
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteAssignmentSubmission(Guid id, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        //}
    }
}