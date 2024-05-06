using Application.Learn.Lessons;
using Application.Learn.Lessons.Command;
using Application.Learn.Lessons.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Learn;
public class LessonsController : BaseApiController
{
    /** Get All Lesson By Admin **/
    [Authorize(Policy = "RequireRole1")]
    [HttpGet]
    public async Task<IActionResult> GetLessons(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListLesson.Query(), ct));
    }

    /** Get Lesson By LessonId **/
    [Authorize(Policy = "RequireRole1")]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetLesson(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DetailsLesson.Query { LessonId = id }, ct));
    }

    /** Get Lesson By ClassRoomId **/
    [Authorize(Policy = "RequireRole3")]
    [HttpGet("lessonClassRoomId")]
    public async Task<IActionResult> GetLessonsByClassRoomId(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new LessonByClassRoomId.Query(), ct));
    }

    /** Get Lesson By TeacherId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpGet("lessonTeacherId")]
    public async Task<IActionResult> GetLessonsByTeacherId(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new LessonByTeacherId.Query(), ct));
    }

    /** Create Lesson By Admin **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPost]
    public async Task<IActionResult> CreateLessonDto(LessonCreateAndEditDto lessonDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateLesson.Command { LessonCreateAndEditDto = lessonDto }, ct));
    }

    /** Update Lesson By Admin **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditLessonDto(Guid id, LessonCreateAndEditDto lessonDto, CancellationToken ct)
    {
        var result = await Mediator.Send(new EditLesson.Command { LessonId = id, LessonCreateAndEditDto = lessonDto }, ct);
        return HandleResult(result);
    }

    /** Deactivate Lesson By Admin **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPut("deactivate/{id}")]
    public async Task<IActionResult> DeactivateLessonDto(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateLesson.Command { LessonId = id }, ct);
        return HandleResult(result);
    }

    /** Delete Lesson By Admin **/
    [Authorize(Policy = "RequireRole1")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLesson(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DeleteLesson.Command { Id = id }, ct));
    }
}