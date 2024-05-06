namespace Application.User.DTOs;
public class TeacherDto
{
    public int Role { get; set; }
    public string Username { get; set; }
    public string NameTeacher { get; set; }
    public DateOnly BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Nip { get; set; }
    public string Token { get; set; }
}

public class TeacherRegisterDto
{
    public int Role { get; set; }
    public string Username { get; set; }
    public string NameTeacher { get; set; }
    public DateOnly BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Nip { get; set; }
}

public class TeacherGetAllAndByIdDto
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public string NameTeacher { get; set; }
    public DateOnly BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Nip { get; set; }
    public string Gender { get; set; }
    public ICollection<string> LessonNames { get; set; }
    public ICollection<string> ClassNames { get; set; }
}

public class TeacherGetLessonDto
{
    public Guid TeacherId { get; set; }
    public string NameTeacher { get; set; }
}