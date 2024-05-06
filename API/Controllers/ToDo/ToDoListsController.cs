using Application.ToDo.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.ToDo
{
    public class ToDoListsController : BaseApiController
    {
        /** Get All ToDoList **/
        [Authorize(Policy = "RequireRole1")]
        [HttpGet]
        public async Task<IActionResult> GetToDoLists(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new GetAllToDoList.Query(), ct));
        }
    }
}
