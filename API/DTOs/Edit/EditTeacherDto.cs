using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Edit
{
    public class EditTeacherDto
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public ICollection<Guid> LessonIds { get; set; }
    }
}
