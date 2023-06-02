namespace LicentaBun.Models
{
    public class SentimentCSV
    {
        public string DateTime { get; set; }
        public string TweetId { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public string Language { get; set; }
        public string? Hashtags { get; set; }
        public int ReplyCount { get; set; }
        public int RetweetCount { get; set; }
        public int LikeCount { get; set; }
        public int QuoteCount { get; set; }
        public string Sentiment { get; set; }
        public float Value { get; set; }

    }
}
