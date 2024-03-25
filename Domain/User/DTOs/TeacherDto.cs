namespace Domain.User.DTOs
{
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

    public class TeacherGetDto
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
}
