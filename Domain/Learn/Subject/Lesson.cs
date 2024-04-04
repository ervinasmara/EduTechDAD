using Domain.Learn.Agenda;
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
        
        // Relasi dengan jadwal
        public ICollection<Schedule> Schedules { get; set; }
    }
}
