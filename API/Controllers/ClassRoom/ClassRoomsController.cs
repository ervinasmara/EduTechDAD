using Application.ClassRooms;
using Application.ClassRooms.Command;
using Application.ClassRooms.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.ClassRoom;
public class ClassRoomsController : BaseApiController
{
    /** Get All ClassRoom **/
    [Authorize(Policy = "RequireRole1")]
    [HttpGet]
    public async Task<IActionResult> GetClassRooms(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ListClassRoom.Query(), ct));
    }

    /** Get ClassRoom By TeacherId **/
    [Authorize]
    [HttpGet("classRoomTeacherId")]
    public async Task<IActionResult> GetClassRoomTeacher(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new ClassRoomTeacher.Query(), ct));
    }

    /** Get ClassRoom By ClassRoomId **/
    [Authorize(Policy = "RequireRole1")]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetClassRoom(Guid id, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new DetailsClassRoom.Query { ClassRoomId = id }, ct));
    }

    /** Create ClassRoom **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPost]
    public async Task<IActionResult> CreateClassRoomDto(ClassRoomCreateAndEditDto classRoomDto, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new CreateClassRoom.Command { ClassRoomCreateAndEditDto = classRoomDto }, ct));
    }

    /** Edit ClassRoom **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditClassRoomDto(Guid id, ClassRoomCreateAndEditDto classRoomDto, CancellationToken ct)
    {
        var result = await Mediator.Send(new EditClassRoom.Command { ClassRoomId = id, ClassRoomCreateAndEditDto = classRoomDto }, ct);

        return HandleResult(result);
    }

    /** Deactivate ClassRoom By ClassRoomId **/
    [Authorize(Policy = "RequireRole1")]
    [HttpPut("deactive/{id}")]
    public async Task<IActionResult> DeactiveClassRoomDto(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactiveClassRoom.Command { ClassRoomId = id }, ct);

        return HandleResult(result);
    }
}