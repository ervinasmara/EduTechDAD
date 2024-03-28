using Microsoft.AspNetCore.Http;

namespace Domain.Learn.Study
{
    public class CourseDto
    {
        public string CourseName { get; set; }
        public string Description { get; set; }
        public byte[] FileData { get; set; }
        public string? LinkCourse { get; set; }
        public string UniqueNumber { get; set; }
    }

    public class CourseEditDto
    {
        public string CourseName { get; set; }
        public string Description { get; set; }
        public IFormFile FileData { get; set; } // Menggunakan IFormFile untuk mengonsumsi file dari permintaan HTTP
        public string LinkCourse { get; set; }
        public string UniqueNumber { get; set; }
    }


    public class CourseGetAllDto
    {
        public Guid Id { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public byte[] FileData { get; set; }
        public string? LinkCourse { get; set; }
        public string UniqueNumber { get; set; }
    }
}
