using Application.Learn.GetFileName;
using Microsoft.AspNetCore.Http;

namespace Application.Learn.Courses
{
    public class CourseCreateAndEditDto
    {
        public string CourseName { get; set; }
        public string Description { get; set; }
        [AllowedExtensions(new string[] { ".pdf" })]
        public IFormFile FileData { get; set; }
        public string LinkCourse { get; set; }
        public string LessonName { get; set; }
    }


    public class CourseGetDto
    {
        public Guid Id { get; set; }
        public string NameTeacher { get; set; }
        public string LessonName { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string LinkCourse { get; set; }
        public string ClassName { get; set; }
        public string LongClassName { get; set; }
        public string FileData { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
