namespace api_BPhotos.Models
{
    public class Album
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
