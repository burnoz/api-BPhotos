using Microsoft.AspNetCore.Mvc;
using api_BPhotos.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net;

namespace api_BPhotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController : ControllerBase
    {
        //private readonly string _storagePath = "";
        private readonly string _storagePath = "";


        [HttpGet]
        public IEnumerable<Photo> GetPhotos(int page = 1, int pageSize = 30)
        {
            var photos = new List<Photo>();
            var imageFiles = Directory.GetFiles(_storagePath, "*.*", SearchOption.TopDirectoryOnly)
                                      .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToList();

            foreach (var filePath in imageFiles)
            {
                var fileInfo = new FileInfo(filePath);
                var fileTitle = Path.GetFileName(filePath);
                var encodedTitle = WebUtility.UrlEncode(fileTitle);

                photos.Add(new Photo
                {
                    Title = fileTitle,
                    ImageURL = $"http://ip:port/api/photo/show/{encodedTitle}",
                    ImageThumbnailURL = $"http://ip:port/api/photo/thumbnail/{encodedTitle}",
                    DateTaken = fileInfo.CreationTime
                });
            }
            return photos;
        }

        [HttpGet("thumbnail/{encodedTitle}")]
        public async Task<IActionResult> GetThumbnail(string encodedTitle)
        {
            var title = WebUtility.UrlDecode(encodedTitle);

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

        [HttpGet("show/{encodedTitle}")]
        public IActionResult ShowImage(string encodedTitle)
        {
            var title = WebUtility.UrlDecode(encodedTitle);

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
