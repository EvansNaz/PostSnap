using Microsoft.AspNetCore.Identity;
using PostSnap.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostSnap.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 500 characters.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastModifiedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        //Foreign key to the Post
        [Required]
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        //Foreign key to the User who wrote it (nullable)
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
