using Application.ClassRooms;
using Application.ClassRooms.Command;
using Application.ClassRooms.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.ClassRoom
{
    public class ClassRoomsController : BaseApiController
    {
        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet]
        public async Task<IActionResult> GetClassRooms(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [Authorize]
        [HttpGet("classRoomTeacherId")]
        public async Task<IActionResult> GetClassRoomTeacher(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ClassRoomTeacher.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetClassRoom(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateClassRoomDto(ClassRoomCreateAndEditDto classRoomDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateClassRoom.Command { ClassRoomCreateAndEditDto = classRoomDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditClassRoomDto(Guid id, ClassRoomCreateAndEditDto classRoomDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditClassRoom.Command { Id = id, ClassRoomCreateAndEditDto = classRoomDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("deactive/{id}")]
        public async Task<IActionResult> DeactiveClassRoomDto(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new DeactiveClassRoom.Command { Id = id }, ct);

            return HandleResult(result);
        }

        //[Authorize(Policy = "RequireRole1OrRole4")]
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteClassRoom(Guid id, CancellationToken ct)
        //{
        //    return HandleResult(await Mediator.Send(new DeleteClassRoom.Command { Id = id }, ct));
        //}
    }
}