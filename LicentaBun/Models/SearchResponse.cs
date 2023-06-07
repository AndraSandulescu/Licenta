using System.Reflection;

namespace LicentaBun.Models
{
    public class SearchResponse
    {
        public List<SearchCSV> Results { get; set; }
        public int SearchIndex { get; set; }
    }
}
