using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PostSnap.Data;
using PostSnap.Dtos;
using PostSnap.Models;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;

namespace PostSnap.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posts
        public IActionResult Index(string searchTerm, string sortOrder, int? page)
        {
            try
            {
                int pageSize = 5;
                int pageNumber = page ?? 1;

                // Fetch posts that aren't soft-deleted
                IQueryable<Post> postsQuery = _context.Posts
                    .Where(p => !p.IsDeleted)
                    .Include(p => p.User);

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    postsQuery = postsQuery.Where(p =>
                        p.Title.Contains(searchTerm) ||
                        p.Body.Contains(searchTerm));
                }

                // Apply sorting
                postsQuery = sortOrder switch
                {
                    "oldest" => postsQuery.OrderBy(p => p.CreatedAt),
                    "title_asc" => postsQuery.OrderBy(p => p.Title),
                    _ => postsQuery.OrderByDescending(p => p.CreatedAt),
                };

                // Paginate results
                var posts = postsQuery.ToPagedList(pageNumber, pageSize);

                // Pass data to view
                ViewData["CurrentSort"] = sortOrder;
                ViewData["CurrentFilter"] = searchTerm;

                return View(posts);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }


        // GET: Posts/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Handle when ID is null
            }

            try
            {
                // Retrieve the post with related User info
                var post = await _context.Posts
                    .Include(p => p.User) // Load associated User info
                    .FirstOrDefaultAsync(m => m.Id == id); // Get post by ID

                if (post == null)
                {
                    return NotFound(); // Handle when post is not found
                }

                return View(post); // Pass the post to the view
            }
            catch (Exception ex)
            {
                // Log the error (you can log it to a file, database, etc.)
                Console.WriteLine($"Error: {ex.Message}");
                return View("Error"); // Return a generic error view if something goes wrong
            }
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostDto postDto)
        {
            //Get the authenticated user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("login", "Account");

            }

            //Validate ModelState before proceeding
            if (!ModelState.IsValid)
            {
                return View(postDto);
            }

            //Map the Dto to the actual Post model
            var post = new Post
            {
                Title = postDto.Title,
                Body = postDto.Body,
                UserId = userId, //Securely set in the controller
                CreatedAt = DateTime.Now,
                LastModifiedAt = DateTime.Now,
                IsDeleted = false
            };

            //Add the new post to the database
            _context.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await  _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);//shows the post if it is not Deleted

            if(post == null) 
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(post.UserId != userId)
            {
                return Forbid();//if the user that owns the post tries to access the edit view will have a 403(forbidden)
            }
            var dto = new EditPostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body
            };

            return View(dto);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostDto postDto)
        {
          if(id != postDto.Id)
            {
                return BadRequest();
            }

          if(!ModelState.IsValid)
            {
                return View(postDto);
            }

            var post = await _context.Posts.FindAsync(id);
            if(post == null || post.IsDeleted)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(post.UserId != userId)
            {
                return Forbid(); //Only owner can edit
            }
            
            //Update allowed fields
            post.Title = postDto.Title;
            post.Body = postDto.Body;
            post.LastModifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        
        }

        // POST: Posts/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(post == null || post.IsDeleted)
            {
                return NotFound();
            }
            if (post.UserId != userId)
            {
                return Forbid();//Only owner can delete
            }
            
            post.IsDeleted = true;
            post.LastModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}


