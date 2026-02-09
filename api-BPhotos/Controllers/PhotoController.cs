using api_BPhotos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;

namespace api_BPhotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController(AppDbContext context) : ControllerBase
    {
        private readonly string _storagePath = "";
        private readonly AppDbContext _context = context;

        private async Task<string> CalculateHashAsync(IFormFile file)
        {
            using var sha256 = SHA256.Create();
            using var stream = file.OpenReadStream();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos(int page = 1, int pageSize = 30)
        {
            var photosFromDb = await _context.UserPhotos
                .OrderByDescending(p => p.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var photos = new List<Photo>();

            foreach (var dbPhoto in photosFromDb)
            {
                photos.Add(new Photo
                {
                    Title = dbPhoto.OriginalName,
                    ImageURL = $"http://ip:port/api/photo/show/{dbPhoto.Id}",
                    ImageThumbnailURL = $"http://ip:port/api/photo/thumbnail/{dbPhoto.Id}",
                    DateTaken = dbPhoto.DateTaken ?? dbPhoto.UploadDate
                });
            }

            return Ok(photos);
        }

        [HttpGet("thumbnail/{photoId}")]
        public async Task<IActionResult> GetThumbnail(int photoId)
        {
            var userPhoto = await _context.UserPhotos
                .Include(up => up.PhysicalFile)
                .FirstOrDefaultAsync(up => up.Id == photoId);

            if (userPhoto == null || userPhoto.PhysicalFile == null)
            {
                return NotFound("La referencia de la foto no existe.");
            }

            var filePath = Path.Combine(_storagePath, userPhoto.PhysicalFile.RelativePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("El archivo físico no se encuentra en el servidor.");
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

        [HttpGet("show/{photoId}")]
        public async Task<IActionResult> ShowImage(int photoId) {
            var userPhoto = await _context.UserPhotos
                .Include(up => up.PhysicalFile)
                .FirstOrDefaultAsync(up => up.Id == photoId);

            if (userPhoto == null || userPhoto.PhysicalFile == null)
            {
                return NotFound("La referencia de la foto no existe.");
            }

            var filePath = Path.Combine(_storagePath, userPhoto.PhysicalFile.RelativePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("El archivo físico no se encuentra en el servidor.");
            }

            var extension = Path.GetExtension(userPhoto.OriginalName).ToLower();

            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream",
            };

            var imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(imageStream, contentType);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto(IFormFile file, [FromQuery] int userId)
        {
            string fileHash = await CalculateHashAsync(file);

            var existingFile = await _context.PhysicalFiles
                .FirstOrDefaultAsync(f => f.FileHash == fileHash);

            PhysicalFile physicalFile;

            if (existingFile == null)
            {
                var fileName = $"{fileHash}{Path.GetExtension(file.FileName)}";
                var relativePath = Path.Combine("uploads", fileName);
                var fullPath = Path.Combine(_storagePath, relativePath);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                physicalFile = new PhysicalFile
                {
                    FileHash = fileHash,
                    RelativePath = relativePath,
                    SizeInBytes = file.Length
                };
                _context.PhysicalFiles.Add(physicalFile);
                await _context.SaveChangesAsync();
            }
            else
            {
                physicalFile = existingFile;
            }

            var userPhoto = new UserPhoto
            {
                OriginalName = file.FileName,
                PhysicalFileId = physicalFile.Id,
                UserId = userId
            };

            _context.UserPhotos.Add(userPhoto);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Foto gestionada con éxito", isDuplicate = existingFile != null });
        }
    }
}
