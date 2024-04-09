using Domain.Present;
using Domain.User;

namespace Application.Presents.DTOs
{
    public class StudentAttendance
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        public Guid AttendanceId { get; set; }
        public Attendance Attendance { get; set; }
    }
}