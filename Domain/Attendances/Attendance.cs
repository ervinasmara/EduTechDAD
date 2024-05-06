using Domain.User;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Attendances;
public class Attendance
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public int Status { get; set; }

    // Menunjukkan kunci asing ke Student
    public Guid StudentId { get; set; }
    [ForeignKey("StudentId")]
    public Student Student { get; set; }
}