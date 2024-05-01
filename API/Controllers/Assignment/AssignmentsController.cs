using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Assignments.Command;
using Application.Assignments;
using Application.Assignments.Query;

namespace API.Controllers.Assignment
{
    public class AssignmentsController : BaseApiController
    {
        /** Get All Assignment By SuperAdmin **/
        [Authorize(Policy = "RequireRole4")]
        [HttpGet]
        public async Task<IActionResult> GetAssignments(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAssignments.Query(), ct));
        }

        /** Get Assignment By AssignmentId **/
        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAssignmentById(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsAssignment.Query { AssignmentId = id }, ct));
        }

        /** Get Assignment By TeacherId **/
        [Authorize(Policy = "RequireRole2")]
        [HttpGet("getAssignmentByTeacherId")]
        public async Task<IActionResult> GetAssignmentstByTeacherId(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAssignmentsByTeacherId.Query(), ct));
        }

        /** Get Assignment By ClassRoomId **/
        [Authorize(Policy = "RequireRole3")]
        [HttpGet("getAssignmentByClassRoomId")]
        public async Task<IActionResult> GetAssignmentstByClassRoomId(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAssignmentsByClassRoomId.Query(), ct));
        }

        /** Download Assignment By AssignmentId **/
        [Authorize(Policy = "RequireRole2,3,4")]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadAssignment(Guid id)
        {
            var result = await Mediator.Send(new DownloadAssignment.Query { AssignmentId = id });

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

        /** Create Assignment Who TeachertId **/
        [Authorize(Policy = "RequireRole2")]
        [HttpPost]
        public async Task<IActionResult> CreateAssignments([FromForm] AssignmentCreateAndEditDto dto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateAssignment.Command { AssignmentCreateAndEditDto = dto }, ct));
        }

        /** Edit Assignment Who TeachertId **/
        [Authorize(Policy = "RequireRole2")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAssignment(Guid id, [FromForm] AssignmentCreateAndEditDto assignmentDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditAssignment.Command { AssignmentId = id, AssignmentCreateAndEditDto = assignmentDto }, ct);
            return HandleResult(result);
        }

        /** Deactivate Assignment Who TeachertId **/
        [Authorize(Policy = "RequireRole2")]
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateAssignment(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new DeactivateAssignment.Command { AssignmentId = id }, ct);
            return HandleResult(result);
        }

        /** Delete Assignment By SuperAdmin **/
        [Authorize(Policy = "RequireRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DeleteTask.Command { AssignmentId = id }, ct));
        }
    }
}