namespace Domain.Present
{
    public class AttendanceDto
    {
        public DateOnly Date { get; set; }
        public int Status { get; set; }
        public Guid StudentId { get; set; }
    }
}
