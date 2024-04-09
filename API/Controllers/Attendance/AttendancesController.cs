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

        [HttpGet("{year:int}/{month:int}/{day:int}")]
        public async Task<ActionResult> GetAttendanceByDate2(int year, int month, int day, CancellationToken ct)
        {
            var date = new DateOnly(year, month, day);
            return HandleResult(await Mediator.Send(new GetAttendanceByDate.Query { Year = year, Month = month, Day = day }, ct));
        }

        //[HttpGet("{date}")]
        //public async Task<ActionResult> GetAttendanceByDate(DateOnly date, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new GetAttendanceByDate.Query { Date = date }, ct));
        //}

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [HttpGet("byclassroom/{uniqueNumberOfClassRoom}")]
        public async Task<ActionResult> GetAttendance(string uniqueNumberOfClassRoom, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListByClassRoomUniqueNumber.Query { UniqueNumberOfClassRoom = uniqueNumberOfClassRoom }, ct));
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