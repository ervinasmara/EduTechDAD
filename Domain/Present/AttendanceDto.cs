using System.ComponentModel.DataAnnotations;

namespace Domain.Present
{
    public class AttendanceDto
    {
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Range(1, 3, ErrorMessage = "Status must be between 1 and 3")]
        public int Status { get; set; }
        public Guid StudentId { get; set; }
    }
}
