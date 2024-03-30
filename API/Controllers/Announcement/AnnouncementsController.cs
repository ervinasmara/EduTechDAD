using Application.Announcements;
using Domain.Announcement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Announcements
{
    [Authorize(Policy = "RequireRole1OrRole4")]
    public class AnnouncementsController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new List.Query(), ct));
        }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, ct));
        }
    }
}