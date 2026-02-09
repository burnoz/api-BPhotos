namespace api_BPhotos.Models
{
    public class PhysicalFile
    {
        public int Id { get; set; }
        public required string FileHash { get; set; } // SHA-256
        public required string RelativePath { get; set; }
        public long SizeInBytes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
