namespace api_BPhotos.Models
{
    public class Photo
    {
        public required string Title { get; set; }
        public required string ImageURL { get; set; }
        public required DateTime DateTaken { get; set; }
    } 
}
