using Domain.Class;

namespace Persistence.Seed
{
    public class SeedClassRoom
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.ClassRooms.Any()) return;

            var classmajors = new List<ClassRoom>
            {
                new ClassRoom
                {
                    ClassName = "TKJ",
                    UniqueNumberOfClassRoom = "001",
                },
                new ClassRoom
                {
                    ClassName = "TKR",
                    UniqueNumberOfClassRoom = "002",
                },
                new ClassRoom
                {
                    ClassName = "RPL",
                    UniqueNumberOfClassRoom = "003",
                },
            };

            await context.ClassRooms.AddRangeAsync(classmajors);
            await context.SaveChangesAsync();
        }
    }
}