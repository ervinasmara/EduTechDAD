using Domain.User;

namespace Domain.Present
{
    public class StudentAttendance
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        public Guid AttendanceId { get; set; }
        public Attendance Attendance { get; set; }
    }
}