using Application.Learn.Schedules;
using Application.Learn.Schedules.Command;
using Application.Learn.Schedules.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Learn
{

    public class SchedulesController : BaseApiController
    {
        //[AllowAnonymous]
        //[HttpGet("classnames/{lessonName}")]
        //public async Task<ActionResult<List<string>>> GetClassNameByLessonName(string lessonName, CancellationToken cancellationToken)
        //{
        //    var result = await Mediator.Send(new ClassNameByLessonNameQuery(lessonName), cancellationToken);

        //    if (result.IsSuccess)
        //    {
        //        return Ok(result.Value);
        //    }
        //    else
        //    {
        //        return NotFound(result.Error);
        //    }
        //}


        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet]
        public async Task<IActionResult> GetSchedules(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListSchedule.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetScheduleById(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListScheduleById.Query { ScheduleId = id }, ct));
        }

        [Authorize(Policy = "RequireRole3")]
        [HttpGet("studentClassRoomId")]
        public async Task<IActionResult> GetSchedulesByClassRoomId(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListSchedulesByClassRoomId.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateScheduleDto(ScheduleCreateAndEditDto scheduleDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateSchedule.Command { ScheduleCreateAndEditDto = scheduleDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditScheduleDto(Guid id, ScheduleCreateAndEditDto scheduleDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditSchedule.Command { ScheduleId = id, ScheduleCreateAndEditDto = scheduleDto }, ct);

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