using Application.InfoRecaps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.InfoRecaps
{
    [Authorize(Policy = "RequireRole1OrRole4")]
    public class InfoRecapsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetInfoRecaps(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListInfoRecap.Query(), ct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetInfoRecap(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsInfoRecap.Query { Id = id }, ct));
        }

        [HttpPost]
        public async Task<IActionResult> CreateInfoRecapDto(InfoRecapCreateDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateInfoRecap.Command { InfoRecapCreateDto = announcementDto }, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditInfoRecapDto(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditInfoRecap.Command { Id = id }, ct);

            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfoRecap(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DeleteInfoRecap.Command { Id = id }, ct));
        }
    }
}