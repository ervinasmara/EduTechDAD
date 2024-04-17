using Domain.User;

namespace Domain.Learn.Courses
{
    public class TeacherCourse
    {
        // Kunci asing ke Teacher
        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        // Kunci asing ke Course
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }
}
