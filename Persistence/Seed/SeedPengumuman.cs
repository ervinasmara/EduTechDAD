using Domain.Pengumuman;

namespace Persistence.Seed
{
    public class SeedPengumuman
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.Pengumumans.Any()) return;

            var pengumumans = new List<Pengumuman>
            {
                new Pengumuman
                {
                    Deskripsi = "Pengumuman 1",
                    Tanggal = new DateOnly(2024, 3, 20),
                },
                new Pengumuman
                {
                    Deskripsi = "Pengumuman 2",
                    Tanggal = new DateOnly(2025, 3, 20),
                },
            };

            await context.Pengumumans.AddRangeAsync(pengumumans);
            await context.SaveChangesAsync();
        }
    }
}