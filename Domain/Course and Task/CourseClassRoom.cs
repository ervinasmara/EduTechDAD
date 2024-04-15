using Domain.Learn.Courses;
using Domain.Class;

namespace Domain.Course_and_Task
{
    public class CourseClassRoom
    {
        // Foreign key untuk Course
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        // Foreign key untuk Classroom
        public Guid ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; }
    }
}
