using api_BPhotos.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PhysicalFile> PhysicalFiles => Set<PhysicalFile>();
    public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<AlbumPhoto> AlbumPhotos => Set<AlbumPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhysicalFile>()
            .HasIndex(f => f.FileHash)
            .IsUnique();
        
        modelBuilder.Entity<AlbumPhoto>()
            .HasKey(ap => new { ap.AlbumId, ap.UserPhotoId });
    }
}