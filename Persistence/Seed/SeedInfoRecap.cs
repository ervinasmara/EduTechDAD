using Domain.InfoRecaps;

namespace Persistence.Seed
{
    public class SeedInfoRecap
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.InfoRecaps.Any()) return;

            var classmajors = new List<InfoRecap>
            {
                new InfoRecap
                {
                    Description = "Rekap Absensi Kelas TKJ",
                    Status = 1,
                    LastStatusChangeDate = DateTime.UtcNow,
                },
                new InfoRecap
                {
                    Description = "Rekap Absensi Kelas TKR",
                    Status = 1,
                    LastStatusChangeDate = DateTime.UtcNow,
                },
                new InfoRecap
                {
                    Description = "Rekap Absensi Kelas RPL",
                    Status = 1,
                    LastStatusChangeDate = DateTime.UtcNow,
                },
            };

            await context.InfoRecaps.AddRangeAsync(classmajors);
            await context.SaveChangesAsync();
        }
    }
}