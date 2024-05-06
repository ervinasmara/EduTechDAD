using Domain.Assignments;
using Domain.Learn.Lessons;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Learn.Courses;
public class Course
{
    public Guid Id { get; set; }
    public string CourseName { get; set; }
    public string Description { get; set; }
    public string FilePath { get; set; }
    public string LinkCourse { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Status { get; set; }

    // Kunci asing ke Lesson
    public Guid LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson Lesson { get; set; }

    // Properti navigasi ke Assignment
    public ICollection<Assignment> Assignments { get; set; }
}