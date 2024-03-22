using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Registration
{
    public class RegisterTeacherDto
    {
        [Required]
        public string NameTeacher { get; set; }

        [Required(ErrorMessage = "BirthDate is required")]
        public DateOnly BirthDate { get; set; }

        [Required]
        public string BirthPlace { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Nip { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [RegularExpression("(?=.*\\d)(?=.[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password Harus Rumit")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(2, 2, ErrorMessage = "Role must be 2")]
        public int Role { get; set; }
    }
}
