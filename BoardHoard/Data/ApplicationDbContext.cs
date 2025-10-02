using BoardHoard.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<WallMessage> WallMessages => Set<WallMessage>();


    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // UserProfile config
        b.Entity<UserProfile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();
            e.Property(p => p.DisplayName).HasMaxLength(64);
            e.HasOne(p => p.User)
             .WithOne()
             .HasForeignKey<UserProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // WallMessage config
        b.Entity<WallMessage>(e =>
        {
            e.Property(m => m.Body).HasMaxLength(1000);

            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(m => m.OwnerUserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(m => m.AuthorUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
