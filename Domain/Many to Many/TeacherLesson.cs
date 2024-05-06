using Domain.Learn.Lessons;
using Domain.User;

namespace Domain.Many_to_Many;
public class TeacherLesson
{
    // Foreign key untuk Teacher
    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; }

    // Foreign key untuk Lesson
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; }
}