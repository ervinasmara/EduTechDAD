using Domain.Assignments;
using Domain.Many_to_Many;
using Domain.Learn.Lessons;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Learn.Courses
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

        // Properti navigasi ke Assignment
        public ICollection<Assignment> Assignments { get; set; }

        // Properti navigasi ke TeacherCourse
        public ICollection<TeacherCourse> TeacherCourses { get; set; }

        // Relasi many-to-many dengan Course melalui tabel pivot CourseClassRoom
        public ICollection<CourseClassRoom> CourseClassRooms { get; set; }
    }
}
