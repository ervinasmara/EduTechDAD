using Microsoft.AspNetCore.Http;

namespace Domain.Submission
{
    public class AssignmentSubmissionGetByIdAssignmentDto
    {
        public DateTime SubmissionTime { get; set; }
        public int Status { get; set; }
        public string Link { get; set; }
        public float Grade { get; set; }
        public string Comment { get; set; }
        public byte[] FileData { get; set; }
    }

    public class AssignmentSubmissionStudentDto
    {
        public IFormFile? FileData { get; set; }
        public string? Link { get; set; }
    }

    public class AssignmentSubmissionTeacherDto
    {
        public float Grade { get; set; }
        public string Comment { get; set; }
    }


    public class AssignmentSubmissionStatusDto
    {
        public int Status { get; set; } // Status tugas
        public Guid StudentId { get; set; } // ID siswa yang mengumpulkan tugas
        public Guid AssignmentId { get; set; } // ID tugas yang dikumpulkan
    }

}
