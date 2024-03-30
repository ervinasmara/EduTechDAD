using Domain.Learn.Study;
using Domain.Submission;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Task
{
    public class Assignment
    {
        public Guid Id { get; set; }
        public string AssignmentName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateOnly AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        public byte[]? FileData { get; set; }
        public string? AssignmentLink { get; set; }
        public int Status { get; set; } = 1;

        // Kunci asing ke Course
        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        // Relasi dengan AssignmentSubmission
        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }
    }
}
