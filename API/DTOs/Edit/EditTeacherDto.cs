namespace API.DTOs.Edit
{
    public class EditTeacherDto
    {
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public ICollection<Guid> LessonIds { get; set; }
    }
}
