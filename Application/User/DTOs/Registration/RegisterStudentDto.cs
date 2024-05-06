using System.ComponentModel.DataAnnotations;

namespace Application.User.DTOs.Registration;
public class RegisterStudentDto
{
    public string NameStudent { get; set; }
    public DateOnly BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string ParentName { get; set; }
    public int Gender { get; set; }
    public string UniqueNumberOfClassRoom { get; set; }
}

public class RegisterStudentExcelDto
{
    [Required]
    public string NameStudent { get; set; }

    [Required(ErrorMessage = "BirthDate is required")]
    public DateOnly BirthDate { get; set; }

    [Required]
    public string BirthPlace { get; set; }

    [Required]
    public string Address { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression("^[0-9]{8,13}$", ErrorMessage = "Phone number must be between 8 and 13 digits and contain only numbers.")]
    public string PhoneNumber { get; set; }

    [Required]
    public string Nis { get; set; }

    [Required]
    public string ParentName { get; set; }
    public int Gender { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    [RegularExpression("(?=.*\\d)(?=.[a-z])(?=.*[A-Z]).{8,16}$", ErrorMessage = "Password Harus Rumit")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [Range(3, 3, ErrorMessage = "Role must be 3")]
    public int Role { get; set; } = 3;

    [Required(ErrorMessage = "UniqueNumberOfClassRoom is required")]
    public string UniqueNumberOfClassRoom { get; set; }
}