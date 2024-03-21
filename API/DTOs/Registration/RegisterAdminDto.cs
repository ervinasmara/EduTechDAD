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
        [RegularExpression("(?=.*\\d)(?=.[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password Harus Rumit")]
        public string Password { get; set; }

        [Required]
        public int Role { get; set; }
    }
}
