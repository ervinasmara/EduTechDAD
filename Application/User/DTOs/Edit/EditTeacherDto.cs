namespace Application.User.DTOs.Edit;
public class EditTeacherDto
{
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public ICollection<string> LessonNames { get; set; }
}