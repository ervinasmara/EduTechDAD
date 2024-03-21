using Domain.Announcement;

namespace Persistence.Seed
{
    public class SeedAnnouncement
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.Announcements.Any()) return;

            var announcements = new List<Announcement>
            {
                new Announcement
                {
                    Description = "Pengumuman 1",
                    Date = new DateOnly(2024, 3, 20),
                },
                new Announcement
                {
                    Description = "Pengumuman 2",
                    Date = new DateOnly(2025, 3, 20),
                },
            };

            await context.Announcements.AddRangeAsync(announcements);
            await context.SaveChangesAsync();
        }
    }
}