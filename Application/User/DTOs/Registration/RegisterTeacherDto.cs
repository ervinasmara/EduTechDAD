using System.ComponentModel.DataAnnotations;

namespace Application.User.DTOs.Registration
{
    public class RegisterTeacherDto
    {
        public string NameTeacher { get; set; }
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Nip { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
        public ICollection<string> LessonName { get; set; }
        public ICollection<string> UniqueNumberOfClassRoom { get; set; }
    }
}
