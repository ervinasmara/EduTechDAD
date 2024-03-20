using Application.Pengumumans;
using Domain.Pengumuman;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace API.Controllers.Pengumumans
{
    public class PengumumansController : BaseApiController
    {

        [HttpGet]
        public async Task<ActionResult<List<Pengumuman>>> GetPengumumans(CancellationToken ct)
        {
            return await Mediator.Send(new List.Query(), ct);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pengumuman>> GetPengumuman(Guid id, CancellationToken ct)
        {
            return await Mediator.Send(new Details.Query { Id = id }, ct);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePengumuman(Pengumuman pengumuman, CancellationToken ct)
        {
            await Mediator.Send(new Create.Command { Pengumuman = pengumuman }, ct);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPengumuman(Guid id, Pengumuman pengumuman, CancellationToken ct)
        {
            pengumuman.Id = id;

            await Mediator.Send(new Edit.Command { Pengumuman = pengumuman }, ct);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id, CancellationToken ct)
        {
            await Mediator.Send(new Delete.Command { Id = id }, ct);
            return Ok();
        }
    }
}