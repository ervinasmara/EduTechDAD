﻿using Application.Learn.GetFileName;
using Microsoft.AspNetCore.Http;

namespace Application.Assignments
{
    public class AssignmentGetAllAndByIdDto
    {
        public Guid Id { get; set; }
        public string AssignmentName { get; set; }
        public string AssignmentFileName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateTime AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        public string AssignmentLink { get; set; }
        public string NameTeacher { get; set; }
        public string LessonName { get; set; }
        public string ClassName { get; set; }
        public string LongClassName { get; set; }
        public string AssignmentStatus { get; set; }
        public string AssignmentFilePath { get; set; }
    }

    public class AssignmentGetByClassRoomIdDto
    {
        public Guid Id { get; set; }
        public string AssignmentName { get; set; }
        public string AssignmentFileName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateTime AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        public string AssignmentLink { get; set; }
        public string LessonName { get; set; }
        public string AssignmentStatus { get; set; }
        public string AssignmentSubmissionStatus { get; set; }
        public string AssignmentFilePath { get; set; }
    }

    public class AssignmentGetByTeacherIdDto
    {
        public Guid AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string AssignmentFileName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateTime AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        public string AssignmentLink { get; set; }
        public Guid LessonId { get; set; }
        public string LessonName { get; set; }
        public string ClassName { get; set; }
        public string AssignmentFilePath { get; set; }
    }

    public class AssignmentCreateAndEditDto
    {
        public string AssignmentName { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateTime AssignmentDeadline { get; set; }
        public string AssignmentDescription { get; set; }
        [AllowedExtensions(new string[] { ".pdf" })]
        public IFormFile AssignmentFileData { get; set; }
        public string AssignmentLink { get; set; }
        public Guid CourseId { get; set; }
    }

    public class DownloadFileDto
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
