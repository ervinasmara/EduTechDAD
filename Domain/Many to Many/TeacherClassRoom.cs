using Domain.Class;
using Domain.User;

namespace Domain.Many_to_Many
{
    public class TeacherClassRoom
    {
        // Foreign key untuk Teacher
        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        // Foreign key untuk ClassRoom
        public Guid ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; }
    }
}
