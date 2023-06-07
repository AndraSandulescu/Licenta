namespace LicentaBun.Models
{
    public class SearchFile
    {
        public string Filename { get; set; }
        public int UserID { get; set; } //userul care a facut cautarea
        public SearchRequest SearchInput {  get; set; }
    }
}
