namespace LicentaBun.Models
{
    public class CompareRequest
    {
        public string? text { get; set; }

        public string? since { get; set; }
        public string? until { get; set; }

        public int? politician1 { get; set; }
        public int? politician2 { get; set; }
    }
}
