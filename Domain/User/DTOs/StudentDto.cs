namespace Domain.User.DTOs
{
    public class StudentDto
    {
        public int Role { get; set; }
        public string Username { get; set; }
        public string NameStudent { get; set; }
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Nis { get; set; }
        public string ParentName { get; set; }
        public int Gender { get; set; }
        public string ClassName { get; set; }
        public string Token { get; set; }
    }

    public class StudentGetDto
    {
        public int Role { get; set; }
        public string Username { get; set; }
        public string NameStudent { get; set; }
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Nis { get; set; }
        public string ParentName { get; set; }
        public string ClassName { get; set; }
        public int Gender { get; set; }
    }

    public class StudentGetAllDto
    {
        public Guid Id { get; set; }
        public string NameStudent { get; set; }
        public string Nis { get; set; }
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string ParentName { get; set; }
        public int Gender { get; set; }
        public string Username { get; set; }
        public int Role { get; set; }
        public string ClassName { get; set; }
        public string UniqueNumber { get; set; }
    }

    public class StudentGetByIdDto
    {
        public string NameStudent { get; set; }
        public string Nis { get; set; }
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string ParentName { get; set; }
        public string Username { get; set; }
        public string ClassName { get; set; }
        public int Gender { get; set; }
    }
}