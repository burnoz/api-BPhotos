using Microsoft.AspNetCore.Mvc;
using api_BPhotos.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace api_BPhotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController : ControllerBase
    {
        //private readonly string _storagePath = "";
        private readonly string _storagePath = "";


        [HttpGet]
        public IEnumerable<Photo> GetLocal()
        {
            var photos = new List<Photo>();
            var imageFiles = Directory.GetFiles(_storagePath, "*.*", SearchOption.TopDirectoryOnly)
                                      .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
            foreach (var filePath in imageFiles)
            {
                var fileInfo = new FileInfo(filePath);
                photos.Add(new Photo
                {
                    Title = Path.GetFileName(filePath),
                    ImageURL = $"http://ip:port/api/photo/show/{Path.GetFileName(filePath)}",
                    ImageThumbnailURL = $"http://ip:port/api/photo/thumbnail/{Path.GetFileName(filePath)}",
                    DateTaken = fileInfo.CreationTime
                });
            }
            return photos;
        }

        [HttpGet("thumbnail/{title}")]
        public async Task<IActionResult> GetThumbnail(string title)
        {
            var filePath = Path.Combine(_storagePath, title);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var thumbnailStream = new MemoryStream();

            using (var image = await Image.LoadAsync(filePath))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Max
                }));
                await image.SaveAsJpegAsync(thumbnailStream);
            }

            thumbnailStream.Position = 0;

            return File(thumbnailStream, "image/jpeg");
        }

        [HttpGet("show/{title}")]
        public IActionResult ShowImage(string title)
        {
            var filePath = Path.Combine(_storagePath, title);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            var extension = Path.GetExtension(filePath).ToLower();
            var imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream",
            };

            return File(imageStream, contentType);
        }
    }
}
