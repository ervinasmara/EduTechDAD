namespace Application.Learn.Schedules;
public class ScheduleCreateAndEditDto
{
    public int Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string LessonName { get; set; }
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