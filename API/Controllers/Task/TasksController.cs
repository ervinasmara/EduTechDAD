using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Assignments;

namespace API.Controllers.Tasks
{
    public class AssignmentsController : BaseApiController
    {
        [Authorize(Policy = "RequireRole4")]
        [HttpGet]
        public async Task<IActionResult> GetAssignments(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListTasks.Query(), ct));
        }

        [Authorize(Policy = "RequireRole2")]
        [HttpGet("getAssignmentByTeacherId")]
        public async Task<IActionResult> GetAssignmentstByTeacherId(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAssignmentByTeacherId.Query(), ct));
        }

        [Authorize(Policy = "RequireRole3")]
        [HttpGet("getAssignmentByClassRoomId")]
        public async Task<IActionResult> GetAssignmentstByClassRoomId(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAssignmentByClassRoomId.Query(), ct));
        }

        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAssignmentById(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsTask.Query { AssignmentId = id }, ct));
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateTasks([FromForm] CreateTask.Command command, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(command, ct));
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAssignment(Guid id, [FromForm] AssignmentDto assignmentDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditTask.Command { Id = id, AssignmentDto = assignmentDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadTask(Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DownloadTask.Query { Id = id }, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                var downloadFile = result.Value;
                var fileBytes = downloadFile.FileData;
                var contentType = downloadFile.ContentType;
                var fileName = downloadFile.FileName; // Nama file yang diunduh berdasarkan AssignmentName

                return File(fileBytes, contentType, fileName);
            }
            else
            {
                return NotFound(result.Error);
            }
        }

        [Authorize(Policy = "RequireRole2OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassRoom(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DeleteTask.Command { Id = id }, ct));
        }
    }
}