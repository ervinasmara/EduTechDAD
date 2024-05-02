using Application.Attendances;
using Application.Attendances.Command;
using Application.Attendances.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Attendance
{
    public class AttendancesController : BaseApiController
    {
        /** Get All Attendance **/
        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet]
        public async Task<IActionResult> GetAttendances(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAttendance.Query(), ct));
        }

        /** Get Attendance By AttendanceId **/
        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsAttendance.Query { AttendanceId = id }, ct));
        }

        /** Calculate Attendance **/
        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet("calculate")]
        public async Task<ActionResult> GetCalculateAttendance([FromQuery] Guid? classRoomId, [FromQuery] string year, [FromQuery] string month, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CalculateAttendance.AttendanceQuery { ClassRoomId = classRoomId, Year = year, Month = month }, ct));
        }

        /** Get Details Attendance By StudentId **/
        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetAttendanceByStudentId(Guid studentId, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetAllByStudentId.Query { StudentId = studentId }, ct));
        }

        /** Create Attendance **/
        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateAttendanceDto([FromQuery] Guid classRoomId, [FromBody] AttendanceCreateDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateAttendance.Command { ClassRoomId = classRoomId, AttendanceCreateDto = announcementDto }, ct));
        }

        /** Edit Attendance **/
        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAttendanceDto(Guid id, AttendanceEditDto announcementEditDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditAttendance.Command { Id = id, AttendanceEditDto = announcementEditDto }, ct);
            return HandleResult(result);
        }
    }
}