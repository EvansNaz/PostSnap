using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostSnap.Data;
using PostSnap.Dtos;
using PostSnap.Models;
using System.Security.Claims;

namespace PostSnap.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCommentDto commentDto)
        {
            //Get authenticated User's Id

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if(!ModelState.IsValid)
            {
                //Go to Details page of the post that was about to get a comment
                TempData["Error"] = "Invalid comment content.";

                return RedirectToAction("Details", "Posts", new {id = commentDto.PostId});
            }

            var comment = new Comment
            {
                Content = commentDto.Content.Trim(),
                CreatedAt = DateTime.Now,
                PostId = commentDto.PostId,
                UserId = userId
            };

            await _context.AddAsync(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new {id =commentDto.PostId});
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(EditCommentDto dto)
        {
            //Get logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

   

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid comment content.";
                return RedirectToAction("Details", "Posts", new { id = dto.PostId});
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == dto.CommentId && !c.IsDeleted);
            //check if changes were made
            //if (dto.Content == comment.Content) return RedirectToAction("Details", "Posts", new { id = dto.PostId });

            if (comment == null || comment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            comment.Content = dto.Content.Trim();
            comment.LastModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment updated successfully!";

            return RedirectToAction("Details", "Posts", new {id=dto.PostId});
        }

        // COMMENT: Comment/Delete/5
        [Authorize(Roles = "Admin,User")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult>Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (comment == null || comment.IsDeleted)
            {
                return NotFound();
            }
            if(comment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();// Only owner and admin can delete
            }

            comment.IsDeleted = true;
            comment.LastModifiedAt = DateTime.Now;
            TempData["Success"] = "Comment deleted successfully";

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost] [ValidateAntiForgeryToken]
        [ActionName("HardDelete")]
        public async Task<IActionResult> HardDelete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if(comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment permanantly deleted";
            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }
    }
}
