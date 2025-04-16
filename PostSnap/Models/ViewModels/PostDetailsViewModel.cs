using X.PagedList;
using PostSnap.Models;
namespace PostSnap.Models.ViewModels
{
    public class PostDetailsViewModel
    {
        public Post Post {  get; set; }
        public IPagedList<Comment> Comments { get; set; }
    }
}
