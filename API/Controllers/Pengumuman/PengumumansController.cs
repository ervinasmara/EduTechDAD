using Application.Pengumumans;
using Domain.Pengumuman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Pengumumans
{
    [AllowAnonymous]
    public class PengumumansController : BaseApiController
    {
        //[Authorize(Policy = "RequireRole1")]
        [HttpGet]
        public async Task<IActionResult> GetPengumumans(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        //[Authorize(Policy = "RequireRole2")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetPengumuman(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePengumumanDto(PengumumanDto pengumumanDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { PengumumanDto = pengumumanDto }, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPengumumanDto(Guid id, PengumumanDto pengumumanDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, PengumumanDto = pengumumanDto }, ct);

            return HandleResult(result);
        }

        //[Authorize(Policy = "RequireRole3")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengumuman(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}