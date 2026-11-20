using api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class ApplicationDBContext : IdentityDbContext<AppUser>
{
    public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupTeam> GroupTeams => Set<GroupTeam>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.Property(r => r.TokenHash).HasMaxLength(64);
            entity.HasIndex(r => r.TokenHash).IsUnique();

            entity.HasOne(r => r.AppUser)
                .WithMany()
                .HasForeignKey(r => r.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Team>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(30);
            entity.HasIndex(t => new { t.AppUserId, t.Name }).IsUnique();

            entity.HasOne(t => t.AppUser)
                .WithMany()
                .HasForeignKey(t => t.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Member>(entity =>
        {
            entity.HasIndex(m => new { m.TeamId, m.Slot }).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("CK_Members_Slot", "[Slot] BETWEEN 1 AND 6"));

            entity.HasOne(m => m.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Group>(entity =>
        {
            entity.Property(g => g.Name).HasMaxLength(30);
            entity.HasIndex(g => new { g.AppUserId, g.Name }).IsUnique();

            // no cascade here: deleting a user already reaches GroupTeams through Teams,
            // and SQL Server rejects a second cascade path to the same table
            entity.HasOne(g => g.AppUser)
                .WithMany()
                .HasForeignKey(g => g.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<GroupTeam>(entity =>
        {
            entity.HasKey(gt => new { gt.GroupId, gt.TeamId });

            entity.HasOne(gt => gt.Group)
                .WithMany(g => g.GroupTeams)
                .HasForeignKey(gt => gt.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gt => gt.Team)
                .WithMany(t => t.GroupTeams)
                .HasForeignKey(gt => gt.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
