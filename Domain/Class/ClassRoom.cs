using Domain.Many_to_Many;
using Domain.Learn.Schedules;
using Domain.User;

namespace Domain.Class
{
    public class ClassRoom
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string UniqueNumberOfClassRoom { get; set; }

        // Relasi dengan siswa
        public ICollection<Student> Students { get; set; }

        // Relasi dengan jadwal
        public ICollection<Schedule> Schedules { get; set; }

        // Relasi many-to-many dengan ClassRoom melalui tabel pivot CourseClassRoom
        public ICollection<CourseClassRoom> CourseClassRooms { get; set; }

        // Relasi many-to-many dengan ClassRoom melalui tabel pivot CourseClassRoom
        public ICollection<AssignmentClassRoom> AssignmentClassRooms { get; set; }

        // Relasi many-to-many dengan ClassRoom melalui tabel pivot TeacherClassRoom
        public ICollection<TeacherClassRoom> TeacherClassRooms { get; set; }
    }
}