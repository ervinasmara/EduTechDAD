using Microsoft.AspNetCore.Http;

namespace Application.Learn.Courses
{
    public class CourseDto
    {
        public string CourseName { get; set; }
        public string Description { get; set; }
        public IFormFile FileData { get; set; }
        public string LinkCourse { get; set; }
        public string UniqueNumberOfLesson { get; set; }
        public ICollection<string> UniqueNumberOfClassRooms { get; set; }
    }

    public class CourseEditDto
    {
        public string CourseName { get; set; }
        public string Description { get; set; }
        public IFormFile FileData { get; set; }
        public string LinkCourse { get; set; }
        public string UniqueNumberOfLesson { get; set; }
    }


    public class CourseGetDto
    {
        public Guid Id { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string LinkCourse { get; set; }
        public string UniqueNumberOfLesson { get; set; }
        public ICollection<string> UniqueNumberOfClassRooms { get; set; }
        public byte[] FileData { get; set; }
    }
}
