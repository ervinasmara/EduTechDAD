using Domain.Learn.Courses;
using Domain.Submission;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Assignments;
public class Assignment
{
    public Guid Id { get; set; }
    public string AssignmentName { get; set; }
    public DateOnly AssignmentDate { get; set; }
    public DateTime AssignmentDeadline { get; set; }
    public string AssignmentDescription { get; set; }
    public string FilePath { get; set; } // Menyimpan PATH file
    public string AssignmentLink { get; set; }
    public int TypeOfSubmission { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Status { get; set; }

    // Kunci asing ke Course
    public Guid CourseId { get; set; }
    [ForeignKey("CourseId")]
    public Course Course { get; set; }

    // Relasi dengan AssignmentSubmission
    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }
}