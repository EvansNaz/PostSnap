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
    }
}
