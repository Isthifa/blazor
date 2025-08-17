// AppDbContext.cs
using BlazorWebPoc.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebPoc.ApiService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserAccounts> UserAccounts { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<UserToken> UserTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure UserRoles table
            modelBuilder.Entity<Model.UserRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId);
        }
    }
}
