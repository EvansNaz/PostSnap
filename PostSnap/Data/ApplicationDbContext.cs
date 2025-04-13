using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostSnap.Models;
using System.Reflection.Emit;

namespace PostSnap.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure IdentityDbContext configurations are set

            // Apply soft delete globally for Posts
            modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted); // Exclude deleted posts by default

            // Apply soft delete globally for Comments
            modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);


            //1 User has many posts, each post has 1 User
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);// Avoid cascade conflicts

            //1 Post has many comments, each comment belongs to 1 post
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade); //When posts are Deleted so are the comments

            // 1 User has many comments, each comment belongs to 1 user
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Keep comments if user is deleted
        }


    }
}
