using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Registration
{
    public class RegisterAdminDto
    {
        [Required]
        public string NameAdmin { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [RegularExpression("(?=.*\\d)(?=.[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password must be complex")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(1, 1, ErrorMessage = "Role must be 1")]
        public int Role { get; set; }
    }
}
