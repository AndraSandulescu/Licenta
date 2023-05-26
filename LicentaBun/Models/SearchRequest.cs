namespace LicentaBun.Models
{
    public class SearchRequest
    {
        public string? text { get; set; }
        public string? username { get; set; }
        public string? until { get; set; }
        public string? since { get; set; }
        public string? retweet { get; set; }
        public string? replies { get; set; }
        public int? count { get; set; }
    }
}
