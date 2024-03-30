using Domain.Learn.Study;

namespace Domain.Learn.Subject
{
    public class Lesson
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }

        // Properti navigasi ke Course
        public ICollection<Course> Courses { get; set; }
    }
}
