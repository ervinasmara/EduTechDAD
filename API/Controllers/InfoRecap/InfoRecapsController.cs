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
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetInfoRecap(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [HttpPost]
        public async Task<IActionResult> CreateInfoRecapDto(InfoRecapDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { InfoRecapDto = announcementDto }, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditInfoRecapDto(Guid id, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id }, ct);

            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfoRecap(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}