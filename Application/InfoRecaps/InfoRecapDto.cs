using System.Text.Json.Serialization;

namespace Application.InfoRecaps
{
    public class InfoRecapCreateDto
    {
        public string Description { get; set; }
        public int Status { get; set; }

        [JsonIgnore]
        public DateTime LastStatusChangeDate { get; set; }
    }
}
