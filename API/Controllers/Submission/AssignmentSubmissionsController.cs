using Application.Submission;
using Domain.Submission;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Submissions
{
    //[Authorize(Policy = "RequireRole1OrRole4")]
    [AllowAnonymous]
    public class AssignmentSubmissionsController : BaseApiController
    {
        //[HttpGet]
        //public async Task<IActionResult> GetAssignmentSubmissions(CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new List.Query(), ct));
        //}

        //[HttpGet("{id}")]
        //public async Task<ActionResult> GetAssignmentSubmissionByAssignmentId(Guid id, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new GetAssignmentSubmissionByAssignmentId.Query { AssignmentId = id }, ct));
        //}

        [HttpGet("{assignmentId}")]
        public async Task<ActionResult<AssignmentSubmissionGetByIdAssignmentDto>> GetAssignmentSubmissionByAssignmentId(Guid assignmentId, [FromServices] IMediator mediator)
        {
            var result = await mediator.Send(new GetAssignmentSubmissionByAssignmentId.Query { AssignmentId = assignmentId });

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return NotFound(result.Error);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignmentSubmission(AssignmentSubmissionStatusDto assignmentSubmissionStatusDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateStatus.Command { AssignmentSubmissionStatusDto = assignmentSubmissionStatusDto }, ct));
        }

        [HttpPut("teacher/{id}")]
        public async Task<IActionResult> EditAssignmentSubmissionStudent(Guid id, AssignmentSubmissionTeacherDto announcementSubmissionTeacherDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new UpdateTeacher.Command { Id = id, AssignmentSubmissionTeacherDto = announcementSubmissionTeacherDto }, ct);

            return HandleResult(result);
        }

        [HttpPut("student/{id}")]
        public async Task<IActionResult> EditAssignmentSubmissionStudent(Guid id, [FromForm] AssignmentSubmissionStudentDto assignmentSubmissionStudentDto)
        {
            var command = new UpdateStudent.Command
            {
                Id = id,
                AssignmentSubmissionStudentDto = assignmentSubmissionStudentDto
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                var updatedDto = result.Value;
                return Ok(updatedDto); // Mengembalikan respons 200 OK dengan data yang telah diubah
            }
            else
            {
                return BadRequest(result.Error); // Mengembalikan respons 400 Bad Request dengan pesan kesalahan jika edit gagal
            }
        }

        [HttpPut("1/{id}")]
        public async Task<IActionResult> EditAssignmentSubmission(Guid id, AssignmentSubmissionStudentDto assignmentSubmissionStudentDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new UpdateStudent1.Command { Id = id, AssignmentSubmissionStudentDto = assignmentSubmissionStudentDto }, ct);

            if (result.IsSuccess)
            {
                // Jika berhasil, kirimkan data yang telah diubah
                return Ok(result.Value); // Atau Ok(result.Value) jika ingin mengirimkan status 200 OK
            }
            else
            {
                // Jika gagal, kirimkan pesan kesalahan
                return BadRequest(result.Error); // Atau NotFound(result.Error) atau lainnya sesuai dengan kebutuhan
            }
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteAssignmentSubmission(Guid id, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        //}
    }
}