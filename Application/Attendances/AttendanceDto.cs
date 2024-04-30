using System.ComponentModel.DataAnnotations;

namespace Application.Attendances
{
    /// //////////////////////////////// ///
    /** Attendance Create **/
    public class AttendanceDto
    {
        public DateOnly Date { get; set; }
        public ICollection<AttendanceStudentCreateDto> AttendanceStudentCreate { get; set; }
    }

    public class AttendanceStudentCreateDto
    {
        public int Status { get; set; }
        public Guid StudentId { get; set; }
    }

    public class AttendanceEditDto
    {
        public DateOnly Date { get; set; }
        public int Status { get; set; }
    }

    public class AttendanceGetAllDto
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public int Status { get; set; }
    }

    public class AttendanceGetDto
    {
        public Guid StudentId { get; set; }
        public string NameStudent { get; set; }
        public string UniqueNumberOfClassRoom { get; set; }
        public ICollection<AttendanceStudentDto> AttendanceStudent { get; set; }
    }

    public class AttendanceGetByIdDto
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public int Status { get; set; }
        public Guid StudentId { get; set; }
    }

    public class AttendanceStudentDto
    {
        public Guid AttendanceId { get; set; }
        public DateOnly Date { get; set; }
        public int Status { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int PresentCount { get; set; }
        public int ExcusedCount { get; set; }
        public int AbsentCount { get; set; }
    }
}
