using FikirHavuzu.Entities;
using Microsoft.EntityFrameworkCore;

namespace FikirHavuzu.DataAccess;

public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=LAPTOP-DC25M24D\SQLEXPRESS;Database=FikirHavuzuDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Idea> Ideas { get; set; }
    public DbSet<Authority> Authorities { get; set; }
    public DbSet<UserAuthority> UserAuthorities { get; set; }
    public DbSet<IdeaEvaluation> IdeaEvaluations { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<PurchasedItem> PurchasedItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAuthority>()
            .HasKey(ua => new { ua.UserId, ua.AuthorityId });

        modelBuilder.Entity<UserAuthority>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAuthorities)
            .HasForeignKey(ua => ua.UserId);

        modelBuilder.Entity<UserAuthority>()
            .HasOne(ua => ua.Authority)
            .WithMany(a => a.UserAuthorities)
            .HasForeignKey(ua => ua.AuthorityId);

        modelBuilder.Entity<IdeaEvaluation>()
            .HasOne(e => e.Idea)
            .WithMany(i => i.Evaluations)
            .HasForeignKey(e => e.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IdeaEvaluation>()
            .HasOne(e => e.EvaluatorUser)
            .WithMany()
            .HasForeignKey(e => e.EvaluatorUserId)
            .OnDelete(DeleteBehavior.NoAction);

        base.OnModelCreating(modelBuilder);
    }
}