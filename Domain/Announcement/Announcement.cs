namespace Domain.Announcement
{
    public class Announcement
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateOnly Date { get; set; }
    }
}
