using Application.Learn.Schedules;
using Application.Learn.Schedules.Command;
using Application.Learn.Schedules.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Learn;
public class SchedulesController : BaseApiController
{
    /** Get All Schedule **/
    [Authorize(Policy = "RequireRole1OrRole3")]
    [HttpGet]
    public async Task<IActionResult> GetSchedules(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListSchedule.Query(), ct));
    }

    /** Get Schedule By ScheduleId **/
    [Authorize(Policy = "RequireRole1OrRole3")]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetScheduleById(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListScheduleById.Query { ScheduleId = id }, ct));
    }

    /** Get Schedule By ClassRoomId **/
    [Authorize(Policy = "RequireRole3")]
    [HttpGet("studentClassRoomId")]
    public async Task<IActionResult> GetSchedulesByClassRoomId(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListSchedulesByClassRoomId.Query(), ct));
    }

    /** Create Schedule **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPost]
    public async Task<IActionResult> CreateScheduleDto(ScheduleCreateAndEditDto scheduleDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateSchedule.Command { ScheduleCreateAndEditDto = scheduleDto }, ct));
    }

    /** Edit Schedule **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditScheduleDto(Guid id, ScheduleCreateAndEditDto scheduleDto, CancellationToken ct)
    {
        var result = await Mediator.Send(new EditSchedule.Command { ScheduleId = id, ScheduleCreateAndEditDto = scheduleDto }, ct);

        return HandleResult(result);
    }

    /** Delete Schedule **/
    [Authorize(Policy = "RequireRole1")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DeleteSchedule.Command { ScheduleId = id }, ct));
    }
}