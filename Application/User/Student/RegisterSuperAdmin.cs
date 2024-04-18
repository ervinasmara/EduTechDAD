using System.ComponentModel.DataAnnotations;

namespace Application.User.Student
{
    public class RegisterSuperAdminDtoCoba
    {
        [Required]
        public string NameSuperAdmin { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [RegularExpression("(?=.*\\d)(?=.[a-z])(?=.*[A-Z]).{8,16}$", ErrorMessage = "Password must be complex")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(4, 4, ErrorMessage = "Role must be 4")]
        public int Role { get; set; }
    }
}
