using Microsoft.AspNetCore.Identity;
using PostSnap.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostSnap.Models
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]// Auto-incremented by DB
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public string? ImageFileName { get; set; }

        [Required]
        public PostStatus Status { get; set; } = PostStatus.Published;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastModifiedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        //Foreign key linking to  User(nullable)
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }//lazy loading

        // Comments collection
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

    public enum PostStatus
    {
        Draft,
        Published,
        Locked,
        Deleted,
    }
}
