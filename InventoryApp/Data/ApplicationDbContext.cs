using InventoryApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace InventoryApp.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<InventoryEntity> Inventories => Set<InventoryEntity>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<InventoryTag> InventoryTags => Set<InventoryTag>();
    public DbSet<InventoryAccess> InventoryAccesses => Set<InventoryAccess>();
    public DbSet<CustomIdFormat> CustomIdFormats => Set<CustomIdFormat>();
    public DbSet<InventorySequence> InventorySequences => Set<InventorySequence>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<InventoryEntity>()
            .Property(x => x.xmin)
            .IsRowVersion();

        b.Entity<Item>()
            .Property(x => x.xmin)
            .IsRowVersion();

        b.Entity<Item>()
            .HasIndex(x => new { x.InventoryId, x.CustomId })
            .IsUnique();

        b.Entity<Like>()
            .HasIndex(x => new { x.ItemId, x.UserId })
            .IsUnique();

        b.Entity<InventoryTag>()
            .HasIndex(x => new { x.InventoryId, x.TagId })
            .IsUnique();

        b.Entity<InventoryAccess>()
            .HasIndex(x => new { x.InventoryId, x.UserId })
            .IsUnique();

        b.Entity<CustomIdFormat>()
            .HasKey(x => x.InventoryId);

        b.Entity<InventorySequence>()
            .HasKey(x => x.InventoryId);

        b.Entity<Post>()
            .HasIndex(p => new { p.InventoryId, p.CreatedAt });

        b.Entity<InventoryTag>()
            .HasOne<InventoryEntity>()
            .WithMany()
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<InventoryTag>()
            .HasOne<Tag>()
            .WithMany()
            .HasForeignKey(it => it.TagId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}