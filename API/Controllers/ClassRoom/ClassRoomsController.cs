using Application.ClassRooms;
using Domain.Class;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.ClassRooms
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
        public async Task<IActionResult> CreateClassRoomDto(ClassRoomDto classRoomDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { ClassRoomDto = classRoomDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditClassRoomDto(Guid id, ClassRoomDto classRoomDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, ClassRoomDto = classRoomDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassRoom(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}