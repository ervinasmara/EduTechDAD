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
                UniqueNumberOfLesson = "01",
            },
            new Lesson
            {
                LessonName = "Pemograman Dasar",
                UniqueNumberOfLesson = "02",
            },
            new Lesson
            {
                LessonName = "Teknologi Dasar Otomotif",
                UniqueNumberOfLesson = "03",
            },
        };

        await context.Lessons.AddRangeAsync(classmajors);
        await context.SaveChangesAsync();
    }
}