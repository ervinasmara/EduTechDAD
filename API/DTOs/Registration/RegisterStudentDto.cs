using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Registration
{
    public class RegisterStudentDto
    {
        [Required]
        public string NameStudent { get; set; }

        [Required(ErrorMessage = "BirthDate is required")]
        public DateOnly BirthDate { get; set; }

        [Required]
        public string BirthPlace { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Nis { get; set; }

        [Required]
        public string ParentName { get; set; }

        public enum GenderEnum
        {
            Male = 1,
            Female = 2
        }
        [Required(ErrorMessage = "Gender is required")]
        [EnumDataType(typeof(GenderEnum), ErrorMessage = "Invalid Gender value. Use 1 for Male, 2 for Female")]
        public int Gender { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [RegularExpression("(?=.*\\d)(?=.[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password Harus Rumit")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(3, 3, ErrorMessage = "Role must be 3")]
        public int Role { get; set; }

        [Required(ErrorMessage = "UniqueNumberOfClassRoom is required")]
        public string UniqueNumberOfClassRoom { get; set; }
    }
}
