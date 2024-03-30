using Domain.Class;
using Domain.Learn.Study;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Learn.Agenda
{
    public class Schedule
    {
        public Guid Id { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Kunci asing ke Course (Materi)
        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        // Kunci asing ke ClassRoom (Kelas)
        public Guid ClassRoomId { get; set; }
        [ForeignKey("ClassRoomId")]
        public ClassRoom ClassRoom { get; set; }
    }
}
