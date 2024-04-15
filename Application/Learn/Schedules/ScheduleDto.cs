namespace Application.Learn.Schedules
{
    public class ScheduleDto
    {
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid LessonId { get; set; }
        public Guid ClassRoomId { get; set; }
    }

    public class ScheduleGetDto
    {
        public Guid Id { get; set; }
        public int Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid LessonId { get; set; }
        public Guid ClassRoomId { get; set; }
        public TeacherScheduleDto TeacherSchedule { get; set; }
    }

    public class TeacherScheduleDto
    {
        public Guid TeacherId { get; set; }
        public string NameTeacher { get; set; }
    }
}