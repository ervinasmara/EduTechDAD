using Domain.Class;
using Domain.Learn.Lessons;

namespace Domain.Many_to_Many
{
    public class LessonClassRoom
    {
        // Foreign key untuk Lesson
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; }

        // Foreign key untuk Classroom
        public Guid ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; }
    }
}
