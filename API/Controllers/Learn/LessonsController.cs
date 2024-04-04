using Application.Learn.Subject;
using Domain.Learn.Subject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Lessons
{
    public class LessonsController : BaseApiController
    {
        [Authorize(Policy = "RequireRole1,2,3,4")]
        [HttpGet]
        public async Task<IActionResult> GetLessons(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1,2,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetLesson(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateLessonDto(LessonCreateDto lessonDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { LessonCreateDto = lessonDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditLessonDto(Guid id, LessonDto lessonDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, LessonDto = lessonDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}