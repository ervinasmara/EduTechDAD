using Application.Announcements;
using Application.Announcements.Command;
using Application.Announcements.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Announcements
{

    public class AnnouncementsController : BaseApiController
    {
        [Authorize(Policy = "RequireRole1,2,3,4")]
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements(CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new ListAnnouncement.Query(), ct));
        }

        [Authorize(Policy = "RequireRole1,2,3,4")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAnnouncement(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DetailsAnnouncement.Query { Id = id }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncementDto(AnnouncementDto announcementDto, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new CreateAnnouncement.Command { AnnouncementDto = announcementDto }, ct));
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAnnouncementDto(Guid id, AnnouncementDto announcementDto, CancellationToken ct)
        {
            var result = await Mediator.Send(new EditAnnouncement.Command { Id = id, AnnouncementDto = announcementDto }, ct);

            return HandleResult(result);
        }

        [Authorize(Policy = "RequireRole1OrRole4")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(Guid id, CancellationToken ct)
        {
            return HandleResult(await Mediator.Send(new DeleteAnnouncement.Command { Id = id }, ct));
        }
    }
}