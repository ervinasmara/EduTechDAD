using Application.ToDo;
using Application.ToDo.Command;
using Application.ToDo.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.ToDo;

[Authorize(Policy = "RequireRole1")]
public class ToDoListsController : BaseApiController
{
    /** Get All ToDoList **/
    [HttpGet]
    public async Task<IActionResult> GetToDoLists(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new GetAllToDoList.Query(), ct));
    }

    /** Get ToDoList By Id **/
    [HttpGet("{id}")]
    public async Task<ActionResult> GetActivity(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DetailsToDoList.Query { ToDoListId = id }, ct));
    }

    /** Create ToDoList */
    [HttpPost]
    public async Task<IActionResult> CreateToDoList(ToDoListDto toDoList, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateToDoList.Command { ToDoListDto = toDoList }, ct));
    }

    /** Ceklis ToDoList */
    [HttpPut("{id}")]
    public async Task<IActionResult> EditToDoList(Guid id, ToDoListDto toDoList, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new EditToDoList.Command { ToDoListId = id, ToDoListDto = toDoList }, ct));
    }

    /** Ceklis ToDoList */
    [HttpPut("ceklis/{id}")]
    public async Task<IActionResult> EditToDoListCeklis(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CeklisToDoList.Command { ToDoListId = id }, ct));
    }

    /** Delete ToDoList */
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DeleteToDoList.Command { ToDoListId = id }, ct));
    }
}
