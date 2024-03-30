using Application.Presents;
using Domain.Present;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Attendances
{
    [Authorize(Policy = "RequireRole1OrRole4")]
    public class AttendancesController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAttendances(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [HttpGet("byclassroom/{classRoomId}")]
        public async Task<IActionResult> GetAttendanceByClassRoomId(Guid classRoomId)
        {
            var result = await Mediator.Send(new ListByClassRoomId.Query { ClassRoomId = classRoomId });

            if (result.IsSuccess)
            {
                var attendanceDtos = result.Value;
                if (attendanceDtos != null && attendanceDtos.Any())
                {
                    return Ok(attendanceDtos);
                }
                else
                {
                    return NotFound("No attendance records found for the specified class room.");
                }
            }

            return BadRequest(result.Error);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttendanceDto(AttendanceDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { AttendanceDto = announcementDto }, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditAttendanceDto(Guid id, AttendanceDto announcementDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, AttendanceDto = announcementDto }, ct);

            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}