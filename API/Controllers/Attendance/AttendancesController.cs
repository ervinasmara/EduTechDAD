using Application.Attendances;
using Application.Attendances.Command;
using Application.Attendances.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Attendance
{
    public class AttendancesController : BaseApiController
    {
        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet]
        public async Task<IActionResult> GetAttendances(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAttendance.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsAttendance.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet("calculate/{ClassRoomId?}")]
        public async Task<ActionResult> GetCalculateAttendance(Guid? ClassRoomId, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CalculateAttendance.AttendanceQuery { ClassRoomId = ClassRoomId }, ct));
        }

        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetAttendanceByStudentId(Guid studentId, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetAllByStudentId.Query { StudentId = studentId }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateAttendanceDto(AttendanceDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateAttendance.Command { AttendanceDto = announcementDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAttendanceDto(Guid id, AttendanceEditDto announcementEditDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditAttendance.Command { Id = id, AttendanceEditDto = announcementEditDto }, ct);
            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DeleteAttendance.Command { Id = id }, ct));
        }
    }
}