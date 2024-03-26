using Application.Presents;
using Domain.Present;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Attendances
{
    [AllowAnonymous]
    public class AttendancesController : BaseApiController
    {
        //[Authorize(Policy = "RequireRole1")]
        [HttpGet]
        public async Task<IActionResult> GetAttendances(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        //[Authorize(Policy = "RequireRole2")]
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

        //[Authorize(Policy = "RequireRole3")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}