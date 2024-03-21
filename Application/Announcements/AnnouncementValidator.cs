using Domain.Announcement;
using FluentValidation;

namespace Application.Announcements
{
    public class AnnouncementValidator : AbstractValidator<AnnouncementDto>
    {
        public AnnouncementValidator()
        {
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
        }
    }
}