namespace Application.Attendances;
/// //////////////////////////////// ///
/** Attendance Get All **/
public class AttendanceGetDto
{
    public Guid StudentId { get; set; }
    public string NameStudent { get; set; }
    public string UniqueNumberOfClassRoom { get; set; }
    public ICollection<AttendanceStudentDto> AttendanceStudent { get; set; }
}

public class AttendanceStudentDto
{
    public Guid AttendanceId { get; set; }
    public DateOnly Date { get; set; }
    public int Status { get; set; }
}

/// //////////////////////////////// ///
/** Attendance Get By AttendanceId **/
public class AttendanceGetByIdDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public int Status { get; set; }
    public Guid StudentId { get; set; }
}

/// //////////////////////////////// ///
/** Attendance Get By StudentId **/
public class AttendanceGetByStudentIdDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public int Status { get; set; }
}

/// //////////////////////////////// ///
/** Attendance Create **/
public class AttendanceCreateDto
{
    public DateOnly Date { get; set; }
    public ICollection<AttendanceStudentCreateDto> AttendanceStudentCreate { get; set; }
}

public class AttendanceStudentCreateDto
{
    public int Status { get; set; }
    public Guid StudentId { get; set; }
}

/// //////////////////////////////// ///
/** Attendance Edit **/
public class AttendanceEditDto
{
    public DateOnly Date { get; set; }
    public int Status { get; set; }
}

/// //////////////////////////////// ///
/** Attendance Count **/
public class AttendanceSummaryDto
{
    public int PresentCount { get; set; }
    public int ExcusedCount { get; set; }
    public int AbsentCount { get; set; }
}