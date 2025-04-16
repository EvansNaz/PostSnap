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
                    .Include(p => p.User)
                    .Include(c => c.Comments);

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
                Body = post.Body,
                ImageFileName = post.ImageFileName
                
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

            var post = await _context.Posts.FindAsync(id);//Find the post by id
            if(post == null || post.IsDeleted)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(post.UserId != userId)
            {
                return Forbid(); //Only owner can edit
            }

            //Image Upload 

            if (postDto.ImageUpload != null && postDto.ImageUpload.Length > 0)
            {
                post.ImageFileName = await SaveImageAsync(postDto.ImageUpload, post.ImageFileName);
            }

            //Update allowed fields
            post.Title = postDto.Title;
            post.Body = postDto.Body;
            post.LastModifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Good Edit";

            return RedirectToAction("Details", new { postDto.Id });
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
            TempData["Success"] = "Post deleted successfully!";


            await _context.SaveChangesAsync();

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


    }
}


