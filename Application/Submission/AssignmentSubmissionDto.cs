using Microsoft.AspNetCore.Http;

namespace Application.Submission
{
    public class AssignmentSubmissionGetDto
    {
        public Guid Id { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string Link { get; set; }
        public float Grade { get; set; }
        public string Comment { get; set; }
        public string AssignmentId { get; set; }
        public string StudentId { get; set; }
        public string ClassName { get; set; }
        public byte[] FileData { get; set; }
    }

    public class AssignmentSubmissionGetByIdCRandA
    {
        public Guid Id { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int Status { get; set; }
        public string Link { get; set; }
        public float Grade { get; set; }
        public string Comment { get; set; }
        public string StudentId { get; set; }
        public byte[] FileData { get; set; }
    }

    public class AssignmentSubmissionStudentDto
    {
        public IFormFile FileData { get; set; }
        public string Link { get; set; }
    }

    public class AssignmentSubmissionTeacherDto
    {
        public float Grade { get; set; }
        public string Comment { get; set; }
    }


    public class AssignmentSubmissionStatusDto
    {
        public Guid StudentId { get; set; } // ID siswa yang mengumpulkan tugas
        public Guid AssignmentId { get; set; } // ID tugas yang dikumpulkan
    }

    public class SubmissionCreateDto
    {
        public Guid StudentId { get; set; }
        public Guid AssignmentId { get; set; }
        public IFormFile FileData { get; set; }
        public string Link { get; set; }
    }

    public class SubmissionGetAssignmentNameByClassNameDto
    {
        public string AssignmentName { get; set; }
        public string AssignmentNameLessonCourse { get; set; }
        public string ClassName { get; set; }
    }
}
