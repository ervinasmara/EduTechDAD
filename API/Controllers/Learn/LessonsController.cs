using Application.Learn.Subject;
using Domain.Learn.Subject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Lessons
{
    [Authorize(Policy = "RequireRole1OrRole4")]
    public class LessonsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetLessons(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetLesson(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [HttpPost]
        public async Task<IActionResult> CreateLessonDto(LessonCreateDto lessonDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { LessonCreateDto = lessonDto }, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditLessonDto(Guid id, LessonDto lessonDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, LessonDto = lessonDto }, ct);

            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}