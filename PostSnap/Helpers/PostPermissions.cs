using PostSnap.Models;
using System.Security.Claims;

namespace PostSnap.Helpers
{
    public class PostPermissions
    {
        //Can Create
        public static bool CanCreate(ClaimsPrincipal user)
        {
            return IsAuthenticated(user);
        }

        //Can the User edit
        public static bool CanEdit(Post post, ClaimsPrincipal user)
        {
            return IsOwner(post, user) || IsAdmin(user);
        }

        //Can Soft-delete 
        public static bool CanSoftDelete(Post post, ClaimsPrincipal user)
        {
            return IsOwner(post, user) || IsAdmin(user);
        }

        //Can Hard Delete (Admin Only)
        public static bool CanHardDelete(ClaimsPrincipal user)
        {
            return IsAdmin(user);
        }

        //Check id the post's owner is the User
        public static bool IsOwner(Post post, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return post.UserId == userId;
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