using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostSnap.Models;
using System.Reflection.Emit;

namespace PostSnap.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        //Next is Comments

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure IdentityDbContext configurations are set

            // Apply soft delete globally for Posts
            modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted); // Exclude deleted posts by default

            // Apply soft delete globally for Comments
            // modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
        }


    }
}
