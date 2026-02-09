namespace api_BPhotos.Models
{
    public class UserPhoto
    {
        public int Id { get; set; }
        public required string OriginalName { get; set; }
        public int PhysicalFileId { get; set; }
        public int UserId { get; set; }
        public DateTime? DateTaken { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public PhysicalFile? PhysicalFile { get; set; }
    }
}
