using Domain.Class;
using Domain.Attendances;
using Domain.Submission;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.User;
public class Student
{
    public Guid Id { get; set; }
    public string NameStudent { get; set; }
    public DateOnly BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Nis { get; set; }
    public string ParentName { get; set; }
    public int Gender { get; set; }
    public int Status { get; set; } = 1;

    // Menunjukkan kunci asing ke AppUser
    public string AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public AppUser User { get; set; }

    // Menunjukkan kunci asing ke ClassRoom
    public Guid ClassRoomId { get; set; }
    [ForeignKey("ClassRoomId")]
    public ClassRoom ClassRoom { get; set; }

    // Relasi dengan presensi
    public ICollection<Attendance> Attendances { get; set; }

    // Relasi dengan AssignmentSubmission
    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }
}