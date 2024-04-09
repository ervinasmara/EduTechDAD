using Application.Submission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Submissions
{
    public class AssignmentSubmissionsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAssignmentSubmissions(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListSubmission.Query(), ct));
        }

        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAnnouncement(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsSubmission.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("{classRoomId}/{assignmentId}")]
        public async Task<ActionResult> GetAssignmentSubmissionByClassRoomAndAssignment(Guid classRoomId, Guid assignmentId, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetAssignmentSubmissionByClassRoomAndAssignment.Query { ClassRoomId = classRoomId, AssignmentId = assignmentId }, ct));
        }

        [Authorize(Policy = "RequireRole3OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateAssignmentSubmission(AssignmentSubmissionStatusDto assignmentSubmissionStatusDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateStatus.Command { AssignmentSubmissionStatusDto = assignmentSubmissionStatusDto }, ct));
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpPut("teacher/{assignmentSubmissionId}")]
        public async Task<IActionResult> EditAssignmentSubmissionTeachert(Guid assignmentSubmissionId, AssignmentSubmissionTeacherDto announcementSubmissionTeacherDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new UpdateTeacher.Command { Id = assignmentSubmissionId, AssignmentSubmissionTeacherDto = announcementSubmissionTeacherDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole3OrRole4")]
        [HttpPut("student/{assignmentSubmissionId}")]
        public async Task<IActionResult> EditAssignmentSubmissionStudent(Guid assignmentSubmissionId, AssignmentSubmissionStudentDto announcementSubmissionStudentDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new UpdateStudent.Command { Id = assignmentSubmissionId, AssignmentSubmissionStudentDto = announcementSubmissionStudentDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole3OrRole4")]
        [HttpPut("student/{studentId}/assignment/{assignmentId}")]
        public async Task<IActionResult> EditAssignmentSubmissionStudent(Guid studentId, Guid assignmentId, AssignmentSubmissionStudentDto announcementSubmissionStudentDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new UpdateStudent2Id.Command { StudentId = studentId, AssignmentId = assignmentId, AssignmentSubmissionStudentDto = announcementSubmissionStudentDto }, ct);

            return HandleResult(result);
        }

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