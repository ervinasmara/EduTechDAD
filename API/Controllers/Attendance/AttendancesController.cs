using Application.Presents;
using Application.Presents.DTOs;
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

        [HttpGet("attendanceparam")]
        public async Task<ActionResult> GetAttendanceParam([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? day, [FromQuery] string nameStudent, [FromQuery] string className, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetAttendanceByFilter.Query { Year = year, Month = month, Day = day, NameStudent = nameStudent, ClassName = className }, ct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
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