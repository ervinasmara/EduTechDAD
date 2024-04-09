using Application.Learn.Agenda;
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
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [Authorize(Policy = "RequireRole3OrRole4")]
        [HttpGet("{classRoomId}")]
        public async Task<ActionResult> GetByClassRoomId(Guid classRoomId)
        {
            var query = new Details.Query(classRoomId);
            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value); // Return the list of schedules
            }

            return BadRequest(result.Error); // Return error message if something went wrong
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateScheduleDto(ScheduleDto scheduleDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { ScheduleDto = scheduleDto }, ct));
        }

        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditScheduleDto(Guid id, ScheduleDto scheduleDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, ScheduleDto = scheduleDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}