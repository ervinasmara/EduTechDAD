namespace Application.Learn.Schedules
{
    public class ScheduleDto
    {
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string LessonName { get; set; }
        public string ClassName { get; set; }
    }

    public class ScheduleGetDto
    {
        public Guid Id { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string LessonName { get; set; }
        public string ClassName { get; set; }
        public string NameTeacher { get; set; }
    }

    public class TeacherScheduleDto
    {
        public Guid TeacherId { get; set; }
        public string NameTeacher { get; set; }
    }
}