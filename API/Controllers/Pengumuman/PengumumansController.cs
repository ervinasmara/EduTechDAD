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
        public async Task<IActionResult> GetPengumumans(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetPengumuman(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePengumuman(Pengumuman pengumuman, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { Pengumuman = pengumuman }, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPengumuman(Guid id, Pengumuman pengumuman, CancellationToken ct)
        {
            pengumuman.Id = id;

            return HandleResult(await Mediator.Send(new Edit.Command { Pengumuman = pengumuman }, ct));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengumuman(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}