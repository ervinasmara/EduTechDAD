using Domain.Learn.Schedules;
using Domain.Learn.Courses;
using Domain.Many_to_Many;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Class;

namespace Domain.Learn.Lessons;
public class Lesson
{
    public Guid Id { get; set; }
    public string LessonName { get; set; }
    public int Status { get; set; }

    // Kunci asing ke ClassRoom
    public Guid ClassRoomId { get; set; }
    [ForeignKey("ClassRoomId")]
    public ClassRoom ClassRoom { get; set; }
    // Properti navigasi ke Course
    public ICollection<Course> Courses { get; set; }

    // Relasi dengan jadwal
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    // Relasi many-to-many dengan Lesson melalui tabel pivot TeacherLesson
    public ICollection<TeacherLesson> TeacherLessons { get; set; }
}