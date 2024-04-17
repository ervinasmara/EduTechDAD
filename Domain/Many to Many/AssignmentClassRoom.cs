using Domain.Assignments;
using Domain.Class;

namespace Domain.Many_to_Many
{
    public class AssignmentClassRoom
    {
        // Foreign key untuk Assignment
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        // Foreign key untuk ClassRoom
        public Guid ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; }
    }
}
