namespace Domain.InfoRecaps
{
    public class InfoRecap
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime LastStatusChangeDate { get; set; }
    }
}