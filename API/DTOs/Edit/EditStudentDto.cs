using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Edit
{
    public class EditStudentDto
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string UniqueNumberOfClassRoom { get; set; }
    }
}