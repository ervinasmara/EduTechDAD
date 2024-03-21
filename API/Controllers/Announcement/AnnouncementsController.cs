using Application.Announcements;
using Domain.Announcement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Announcements
{
    [AllowAnonymous]
    public class AnnouncementsController : BaseApiController
    {
        //[Authorize(Policy = "RequireRole1")]
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

        //[Authorize(Policy = "RequireRole2")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAnnouncement(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }, ct));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnnouncementDto(AnnouncementDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Create.Command { AnnouncementDto = announcementDto }, ct));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditAnnouncementDto(Guid id, AnnouncementDto announcementDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new Edit.Command { Id = id, AnnouncementDto = announcementDto }, ct);

            return HandleResult(result);
        }

        //[Authorize(Policy = "RequireRole3")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}