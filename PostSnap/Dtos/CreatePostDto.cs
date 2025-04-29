using PostSnap.Attributes;
using System.ComponentModel.DataAnnotations;

namespace PostSnap.Dtos
{
    public class CreatePostDto
    {
        [Required]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "Title needs to be between 3 to 120 characters")]
        public string Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Body is between 10 and 1000 Characters")]
        public string Body { get; set; }

        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".gif" })]
        [MaxFileSize(2 * 1024 * 1024)] // 2 MB
        public IFormFile? ImageUpload { get; set; }
    }
}
