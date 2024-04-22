using Domain.User;

namespace Domain.Assignments
{
    public class TeacherAssignment
    {
        // Kunci asing ke Teacher
        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        // Kunci asing ke Assignment
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
    }
}
