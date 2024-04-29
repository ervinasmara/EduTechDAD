using System.ComponentModel.DataAnnotations;

namespace Application.User.DTOs.Registration
{
    public class RegisterSuperAdminDto
    {
        public string NameSuperAdmin { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
