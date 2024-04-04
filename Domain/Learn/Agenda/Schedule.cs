using Domain.Class;
using Domain.Learn.Subject;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Learn.Agenda
{
    public class Schedule
    {
        public Guid Id { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Kunci asing ke Lesson (Materi)
        public Guid LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        // Kunci asing ke ClassRoom (Kelas)
        public Guid ClassRoomId { get; set; }
        [ForeignKey("ClassRoomId")]
        public ClassRoom ClassRoom { get; set; }
    }
}
