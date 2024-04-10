using Application.User.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Application.Presents.DTOs
{
    public class AttendanceDto
    {
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Range(1, 3, ErrorMessage = "Status must be between 1 and 3")]
        public int Status { get; set; }
        public Guid StudentId { get; set; }
    }

    public class AttendanceGetDto
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public int Status { get; set; }
        public StudentAttendanceDto StudentAttendance { get; set; }
        public ClassRoomAttendanceDto ClassRoomAttendance { get; set; }
    }

    public class StudentAttendanceDto
    {
        public Guid StudentId { get; set; }
        public string NameStudent { get; set; }
    }

    public class ClassRoomAttendanceDto
    {
        public Guid ClassRoomId { get; set; }
        public string ClassName { get; set; }
        public string UniqueNumberOfClassRoom { get; set; }
    }
}
