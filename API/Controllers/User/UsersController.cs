using Application.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User
{
    [AllowAnonymous]
    public class UsersController : BaseApiController
    {
        //[Authorize(Policy = "RequireRole1")]
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListStudent.Query(), ct));
        }

        [HttpGet("student/{id}")]
        public async Task<ActionResult> GetClassRoom(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsStudent.Query { Id = id }, ct));
        }
    }
}