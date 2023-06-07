namespace LicentaBun.Models
{
    public class ModelResponse
    {
        public List<SentimentCSV> Results { get; set; }
        public int SearchIndex { get; set; }
        public int PosTweets { get; set; }
        public int TotalTweets { get; set; }
        public float SentimentMediu { get; set; }

        public Dictionary<string, double> SentimentPerMonth { get; set; }
        public Dictionary<string, int> TweetsPerMonth { get; set; }

    }
}
