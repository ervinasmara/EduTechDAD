using Domain.Learn.Lessons;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Learn.Schedules;
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
}