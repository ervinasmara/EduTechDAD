using Microsoft.AspNetCore.Http;

namespace Domain.Task
{
    public class AssignmentDto
    {
        public string AssignmentName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateOnly AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        public IFormFile? AssignmentFileData { get; set; }
        public string? AssignmentLink { get; set; }
        public Guid CourseId { get; set; }
    }

    public class AssignmentGetAllDto
    {
        public Guid Id { get; set; }
        public string AssignmentName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateOnly AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        public byte[] AssignmentFileData { get; set; }
        public string AssignmentLink { get; set; }
        public string CourseName { get; set; }
    }

    public class DownloadFileDto
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
    }
}
