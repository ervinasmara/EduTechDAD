using Application.Learn.Schedules;
using Application.Learn.Schedules.Command;
using Application.Learn.Schedules.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Schedules
{

    public class SchedulesController : BaseApiController
    {
        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet]
        public async Task<IActionResult> GetSchedules(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListSchedule.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCourse(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListScheduleById.Query { ScheduleId = id }, ct));
        }

        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet("studentClassRoomId")]
        public async Task<IActionResult> GetSchedulesByClassRoomId(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListScheduleByClassRoomId.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateScheduleDto(ScheduleDto scheduleDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateSchedule.Command { ScheduleDto = scheduleDto }, ct));
        }

        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditScheduleDto(Guid id, ScheduleDto scheduleDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditSchedule.Command { Id = id, ScheduleDto = scheduleDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DeleteSchedule.Command { Id = id }, ct));
        }
    }
}