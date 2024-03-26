namespace Domain.User.DTOs
{
    public class SuperAdminDto
    {
        public int Role { get; set; }
        public string Username { get; set; }
        public string NameSuperAdmin { get; set; }
        public string Token { get; set; }
    }

    public class SuperAdminGetDto
    {
        public int Role { get; set; }
        public string Username { get; set; }
        public string NameSuperAdmin { get; set; }
    }
}
