using Domain.Learn.Schedules;
using Domain.Learn.Courses;
using Domain.User;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Many_to_Many;

namespace Domain.Learn.Lessons
{
    public class Lesson
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }

        // Properti navigasi ke Course
        public ICollection<Course> Courses { get; set; }

        // Relasi dengan jadwal
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        // Relasi many-to-many dengan Lesson melalui tabel pivot TeacherLesson
        public ICollection<TeacherLesson> TeacherLessons { get; set; }

        // Relasi many-to-many dengan ClassRoom melalui tabel pivot LessonClassRoom
        public ICollection<LessonClassRoom> LessonClassRooms { get; set; }
    }
}
