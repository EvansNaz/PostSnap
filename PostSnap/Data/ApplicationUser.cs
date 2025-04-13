using Microsoft.AspNetCore.Identity;
using PostSnap.Models;

namespace PostSnap.Data
{
    public class ApplicationUser : IdentityUser
    {
        //initializing the collections to avoid null references
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}
