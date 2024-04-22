using Domain.Many_to_Many;
using Domain.Learn.Courses;
using Domain.Submission;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Assignments
{
    public class Assignment
    {
        public Guid Id { get; set; }
        public string AssignmentName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateOnly AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        public byte[] FileData { get; set; }
        public string AssignmentLink { get; set; }
        public DateTime CreatedAt { get; set; }

        // Kunci asing ke Course
        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        // Relasi dengan AssignmentSubmission
        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }

        // Relasi many-to-many dengan ClassRoom melalui tabel pivot CourseClassRoom
        public ICollection<AssignmentClassRoom> AssignmentClassRooms { get; set; }

        // Properti navigasi ke TeacherAssignment
        public ICollection<TeacherAssignment> TeacherAssignments { get; set; }
    }
}
