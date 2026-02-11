using api_BPhotos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_BPhotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Album>>> GetAlbums()
        {
            var albums = await _context.Albums.ToListAsync();
            return Ok(albums);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Album>> GetAlbum(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            return Ok(album);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Album>> CreateAlbum(Album album)
        {
            album.CreatedAt = DateTime.UtcNow;
            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAlbum), new { id = album.Id }, album);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{albumId}/photos/{photoId}")]
        public async Task<IActionResult> AddPhotoToAlbum(int albumId, int photoId)
        {
            var alreadyExists = await _context.AlbumPhotos
                .AnyAsync(ap => ap.AlbumId == albumId && ap.UserPhotoId == photoId);

            if (alreadyExists)
            {
                return BadRequest("La foto ya está en el álbum.");
            }

            var AlbumExists = await _context.Albums.AnyAsync(a => a.Id == albumId);
            var PhotoExists = await _context.UserPhotos.AnyAsync(p => p.Id == photoId);

            if (!AlbumExists || !PhotoExists)
            {
                return NotFound("El álbum o la foto no existe.");
            }

            var albumPhoto = new AlbumPhoto
            {
                AlbumId = albumId,
                UserPhotoId = photoId
            };

            try
            {
                _context.AlbumPhotos.Add(albumPhoto);
                await _context.SaveChangesAsync();
                return Ok("Foto agregada al álbum.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al agregar la foto al álbum: {ex.Message}");
            }
        }
    }
}
