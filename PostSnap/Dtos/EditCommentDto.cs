using System.ComponentModel.DataAnnotations;

namespace PostSnap.Dtos
{
    public class EditCommentDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 500 characters.")]
        public string Content { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required] 
        public int CommentId { get; set; }
    }
}
