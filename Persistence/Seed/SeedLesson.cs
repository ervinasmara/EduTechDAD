using Domain.Learn.Lessons;

namespace Persistence.Seed;
public class SeedLesson
{
    public static async Task SeedData(DataContext context)
    {
        if (context.Lessons.Any()) return;

        var classmajors = new List<Lesson>
        {
            new Lesson
            {
                LessonName = "Komputer dan Jaringan Dasar",
            },
            new Lesson
            {
                LessonName = "Pemograman Dasar",
            },
            new Lesson
            {
                LessonName = "Teknologi Dasar Otomotif",
            },
        };

        await context.Lessons.AddRangeAsync(classmajors);
        await context.SaveChangesAsync();
    }
}