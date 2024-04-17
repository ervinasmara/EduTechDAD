using Domain.Assignments;
using Domain.User;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Submission
{
    public class AssignmentSubmission
    {
        public Guid Id { get; set; }
        public DateTime? SubmissionTime { get; set; }
        public int Status { get; set; }
        public byte[]? FileData { get; set; }
        public string? Link { get; set; }
        public float? Grade { get; set; }
        public string? Comment { get; set; }

        // Kunci asing ke Assignment
        public Guid AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; }

        // Kunci asing ke Student
        public Guid StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
    }

}
