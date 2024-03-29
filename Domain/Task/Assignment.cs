using Domain.Learn.Study;
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
        public int? Status { get; set; }

        // Kunci asing ke Course
        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
