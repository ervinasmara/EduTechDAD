using Domain.Learn.Subject;

namespace Persistence.Seed
{
    public class SeedLesson
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.Lessons.Any()) return;

            var classmajors = new List<Lesson>
            {
                new Lesson
                {
                    LessonName = "PAI",
                    UniqueNumberOfLesson = "01",
                },
                new Lesson
                {
                    LessonName = "PPKN",
                    UniqueNumberOfLesson = "02",
                },
                new Lesson
                {
                    LessonName = "Bahasa Indonesia",
                    UniqueNumberOfLesson = "03",
                },
            };

            await context.Lessons.AddRangeAsync(classmajors);
            await context.SaveChangesAsync();
        }
    }
}