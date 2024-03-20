namespace Domain.Pengumuman
{
    public class Pengumuman
    {
        public Guid Id { get; set; }
        public string Deskripsi { get; set; }
        public DateOnly Tanggal { get; set; }
    }
}
