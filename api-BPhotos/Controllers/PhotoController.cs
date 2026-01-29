using Microsoft.AspNetCore.Mvc;
using api_BPhotos.Models;

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
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    ImageURL = $"test/url",
                    DateTaken = fileInfo.CreationTime
                });
            }
            return photos;
        }
    }
}
