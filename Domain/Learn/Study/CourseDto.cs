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
}
