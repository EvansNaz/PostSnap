using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PostSnap.Data;
using PostSnap.Dtos;
using PostSnap.Models;
using PostSnap.Models.ViewModels;
using X.PagedList.EntityFramework;
using X.PagedList.Extensions;
using X.PagedList;


namespace PostSnap.Controllers
{
    [Authorize(Roles = "Admin,User")]

    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }



        // GET: Posts Index
        public IActionResult Index(string searchTerm, string sortOrder, int? page)
        {
            try
            {
                int pageSize = 5;
                int pageNumber = page ?? 1;

                var postsQuery = GetFilteredPosts(searchTerm, sortOrder);


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

        //GET: Posts My Posts(user's post only)

        [Authorize(Roles = "Admin,User")]
        public IActionResult MyPosts(string searchTerm, string sortOrder, int? page)
        {
            try
            {
                int pageSize = 5;
                int pageNumber = page ?? 1;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var postsQuery = GetFilteredPosts(searchTerm, sortOrder, userId);

                var posts = postsQuery.ToPagedList(pageNumber, pageSize);

                ViewData["CurrentSort"] = sortOrder;
                ViewData["CurrentFilter"] = searchTerm;
                ViewData["IsMyPosts"] = true;

                return View("Index", posts); //Reuse Index View if layout is the same
            }
            catch
            {
                return View("Error");
            }
        }

        // GET: Posts/Details/5
        [Authorize(Roles = "Admin,User")]
        public IActionResult Details(int? id, int? page)
        {
            if (id == null)
            {
                return NotFound(); // Handle when ID is null
            }


            //Fetch post if not Deleted
            var post =  _context.Posts
                .Include(p => p.User)
                .FirstOrDefault(m => m.Id == id && !m.IsDeleted);

            if (post == null) return NotFound();

            int pageSize = 5;//how many comments to show per page
            int pageNumber = page ?? 1;//if no page is provided, show page 1

            var commentsQuery = _context.Comments
                .Where(c => c.PostId == id && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)   //Newest first
                .Include(c => c.User);

            //executes the query and paginates it
            var pagedComments =  commentsQuery.ToPagedList(pageNumber, pageSize);

            //We use the model to pass both post and its comments to the view
            var viewModel = new PostDetailsViewModel
            {
                Post = post,
                Comments = pagedComments
            };

            //We pass the id as ViewData so the pagination link includes the id for comment pagination in the details view of a post
            ViewData["PostId"] = id;

            return View(viewModel);
 
        }

        // GET: Posts/Create
        [Authorize(Roles = "Admin,User")]
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
        [Authorize(Roles = "Admin,User")]
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
                Title = postDto.Title.Trim(),
                Body = postDto.Body.Trim(),
                UserId = userId, //Securely set in the controller
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            //Image Upload 

            if (postDto.ImageUpload != null && postDto.ImageUpload.Length > 0)
            {
                post.ImageFileName = await SaveImageAsync(postDto.ImageUpload, post.ImageFileName);
            }

            //Add the new post to the database
            _context.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        // GET: Posts/Edit/5
        [Authorize(Roles = "Admin,User")]
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
            if (post.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();//if the user that owns the post tries to access the edit view will have a 403(forbidden)
            }
            var dto = new EditPostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                ImageFileName = post.ImageFileName
                
            };

            return View(dto);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,User")]
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

            var post = await _context.Posts.FindAsync(id);//Find the post by id
            if(post == null || post.IsDeleted)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(post.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); //Only owner can edit
            }

            //Image Upload 

            if (postDto.ImageUpload != null && postDto.ImageUpload.Length > 0)
            {
                post.ImageFileName = await SaveImageAsync(postDto.ImageUpload, post.ImageFileName);
            }

            //Update allowed fields
            post.Title = postDto.Title.Trim();
            post.Body = postDto.Body.Trim();
            post.LastModifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Edit successful";

            return RedirectToAction("Details", new { postDto.Id });
        }

        // POST: Posts/Delete/5
        [Authorize(Roles = "Admin,User")]
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
            if (post.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();//Only owner can delete
            }
            
            post.IsDeleted = true;
            post.LastModifiedAt = DateTime.Now;
            TempData["Success"] = "Post deleted successfully!";


            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("HardDelete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> HardDelete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if(post == null ) return NotFound();
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Post permanantly deleted";

            return RedirectToAction(nameof(Index));
        }


        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }


        //Image Upload Or Replace Method
        private async Task<string> SaveImageAsync(IFormFile imageUpload, string? existingFileName = null)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadsFolder);//Ensure directory exists by creating it if not

            //Check if there is an image already so that we can delete it
            if (!string.IsNullOrEmpty(existingFileName))
            {
                var existingFilePath = Path.Combine(uploadsFolder, existingFileName);
                if (System.IO.File.Exists(existingFilePath))
                    System.IO.File.Delete(existingFilePath);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageUpload.FileName);//GUID to prevent filename collisions &  preserve the file extension (.jpg, .png, etc.)

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);//We create the full file path where the image will be stored


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageUpload.CopyToAsync(stream);        //This creates a new file (or overwrites if it somehow exists)
            }                                                 //It’s wrapped in a using block to auto-dispose the stream and avoid memory leaks

            return uniqueFileName;
        }

        
        /*
            Returns a query for posts, filtered optionally by search term, sort order and user ID
            Used for both the main post index feed and the My Posts page.
         */
        private IQueryable<Post> GetFilteredPosts (string searchTerm, string sortOrder, string? userId = null)
        {
            //Get posts that are not soft-deleted
            //& include related User and Comments data for each post
            // Build base query with includes and cast to avoid type mismatch issues with Where
            var postsQuery =(IQueryable<Post>) _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Comments);

            // Filter by user ID (only when showing 'MyPosts')
            if (!string.IsNullOrEmpty(userId))
            {
                postsQuery = postsQuery.Where(p => p.UserId == userId);
                // Posts where p.UserId is null will be excluded here

            }
            // If a search term is provided, filter to posts whose title or body contains it
            if (!string.IsNullOrEmpty(searchTerm))
            {
                postsQuery = postsQuery
                    .Where(p => p.Title.Contains(searchTerm) || p.Body.Contains(searchTerm));
            }
            // Apply sort order
            postsQuery = sortOrder switch
            {
                "oldest" => postsQuery.OrderBy(p => p.CreatedAt),                  // Oldest first
                "title_asc" => postsQuery.OrderBy(p => p.Title),                   // Alphabetical by title
                "comment_count" => postsQuery.OrderByDescending(p => p.Comments.Count), // Most comments first
                _ => postsQuery.OrderByDescending(p => p.CreatedAt),              // Default: newest first
            };

            // Return the composed query to be used with pagination later
            return postsQuery;
        }

    }
}


