using Application.Learn.Courses;
using Application.Learn.Courses.Command;
using Application.Learn.Courses.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Learn;
public class CoursesController : BaseApiController
{
    /** Get Course By CourseId **/
    [Authorize(Policy = "RequireRole2OrRole3")]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCourse(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DetailsCourse.Query { CourseId = id }, ct));
    }

    /** Get All Course By ClassRoomId **/
    [Authorize(Policy = "RequireRole3")]
    [HttpGet("getCourseByClassRoomId")]
    public async Task<IActionResult> GetStudentCourses(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListCourseByClassRoomId.Query(), ct));
    }

    /** Get All Course By TeacherId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpGet("getCourseByTeacherId")]
    public async Task<IActionResult> GetTeacherCourses(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListCourseByTeacherId.Query(), ct));
    }

    /** Download Course By CourseId **/
    [Authorize(Policy = "RequireRole2OrRole3")]
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadCourse(Guid id)
    {
        var result = await Mediator.Send(new DownloadCourse.Query { CourseId = id });

        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }

        var downloadFileDto = result.Value;
        if (downloadFileDto.FileData == null || downloadFileDto.ContentType == null)
        {
            return NotFound("File data or content type is null.");
        }

        return File(downloadFileDto.FileData, downloadFileDto.ContentType, downloadFileDto.FileName);
    }

    /** Create Course Who TeacherId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpPost]
    public async Task<IActionResult> CreateCourseDto([FromForm] CourseCreateDto courseDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateCourse.Command { CourseCreateDto = courseDto }, ct));
    }

    /** Edit Course Who TeacherId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditCourse(Guid id, [FromForm] CourseEditDto courseDto, CancellationToken ct)
    {
        var result = await Mediator.Send(new EditCourse.Command { CourseId = id, CourseEditDto = courseDto }, ct);
        return HandleResult(result);
    }

    /** Deactivate Course Who TeacherId **/
    [Authorize(Policy = "RequireRole2")]
    [HttpPut("deactivate/{id}")]
    public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateCourse.Command { CourseId = id }, ct);
        return HandleResult(result);
    }
}