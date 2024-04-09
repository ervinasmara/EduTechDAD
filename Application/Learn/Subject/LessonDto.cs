namespace Application.Learn.Subject
{
    public class LessonDto
    {
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
    }

    public class LessonGetTeacherDto
    {
        public Guid Id { get; set; }
        public string LessonName { get; set; }
        public string UniqueNumberOfLesson { get; set; }
    }

    public class LessonCreateDto
    {
        public string LessonName { get; set; }
        public Guid TeacherId { get; set; }
    }
}
