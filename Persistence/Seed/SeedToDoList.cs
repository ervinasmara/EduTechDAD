using Domain.ToDoList;

namespace Persistence.Seed;
public class SeedToDoList
{
    public static async Task SeedData(DataContext context)
    {
        if (context.ToDoLists.Any()) return;

        var toDoList = new List<ToDoList>
        {
            new ToDoList
            {
                Description = "Merekap Absensi TKJ",
                Status = 1,
                CreatedAt = DateTime.UtcNow.AddHours(7),
            },
            new ToDoList
            {
                Description = "Merekap Absensi TKR",
                Status = 1,
                CreatedAt = DateTime.UtcNow.AddHours(7),
            },
            new ToDoList
            {
                Description = "Merekap Absensi RPL",
                Status = 1,
                CreatedAt = DateTime.UtcNow.AddHours(7),
            },
        };

        await context.ToDoLists.AddRangeAsync(toDoList);
        await context.SaveChangesAsync();
    }
}