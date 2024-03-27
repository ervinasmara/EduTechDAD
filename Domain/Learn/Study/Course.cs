using Domain.Learn.Subject;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Learn.Study
{
    public class Course
    {
        public Guid Id { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public byte[]? FileData { get; set; }
        public string? LinkCourse { get; set; }

        // Kunci asing ke Lesson
        public Guid LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }
    }
}
