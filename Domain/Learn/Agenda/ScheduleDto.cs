namespace Domain.Learn.Agenda
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
    }
}