using PostSnap.Models;
using System.Security.Claims;

namespace PostSnap.Helpers
{
    public class CommentPermissions
    {
        //Can the user edit
        public static bool CanEdit(Comment comment, ClaimsPrincipal user)
        {
            return IsOwner(comment, user) || IsAdmin(user);
        }

        //Can soft-delete
        public static bool CanSoftDelete(Comment comment, ClaimsPrincipal user)
        {
            return IsOwner(comment, user ) || IsAdmin(user);
        }

        //Can Hard Delete (Admin Only)
        public static bool CanHardDelete(Comment comment, ClaimsPrincipal user)
        {
            return IsAdmin(user);
        }

        //Check if the comment's owner is the User
        public static bool IsOwner(Comment comment, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return comment.UserId == userId;
        }

        //Check if the User is an Admin
        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }

        //Check if User is authenticated
        public static bool IsAuthenticated(ClaimsPrincipal user)
        {
            return user.IsInRole("User") || user.IsInRole("Admin");
        }
    }
}
