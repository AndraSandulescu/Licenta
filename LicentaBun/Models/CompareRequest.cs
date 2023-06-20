namespace LicentaBun.Models
{
    public class CompareRequest
    {
        public string? text { get; set; }

        public string? formattedSince { get; set; }
        public string? formattedUntil { get; set; }

        public int? politician1 { get; set; }
        public int? politician2 { get; set; }
    }
}
