using Application.Learn.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User
{
    public class UsersController : BaseApiController
    {
        //[Authorize(Policy = "RequireRole1")]
        //[HttpGet("students")]
        //public async Task<IActionResult> GetStudents(CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new ListStudent.Query(), ct));
        //}

        //[HttpGet("student/{id}")]
        //public async Task<ActionResult> GetStudentById(Guid id, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new DetailsStudent.Query { Id = id }, ct));
        //}

        //[HttpGet("teachers")]
        //public async Task<IActionResult> GetTeachers(CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new ListTeacher.Query(), ct));
        //}

        //[HttpGet("teacher/{id}")]
        //public async Task<ActionResult> GetTeacherById(Guid id, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new DetailsTeacher.Query { Id = id }, ct));
        //}
    }
}