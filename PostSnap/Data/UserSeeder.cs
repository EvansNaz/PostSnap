using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PostSnap.Models;

namespace PostSnap.Data
{
    public class UserSeeder
    {
        // we create users with posts

        public static async Task SeedUserAndPosts(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager)
        {
            //Skip seeding if users already exist
            if (await context.Users.AnyAsync())
                return;

            //Create 5 fake users
            var users = new List<ApplicationUser>();

            for (int i = 0; i < 5; i++)
            {
                var email = new Faker().Internet.Email(provider: "example.com").ToLower().Trim();

                var user = new ApplicationUser
                {
                    Email = email,
                    UserName = email // align with email to avoid confusion
                };

                users.Add(user);
            }

            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, "Password123!");
                if(result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    await userManager.ConfirmEmailAsync(user, token);//Confirm Email

                    await userManager.AddToRoleAsync(user, "User"); //  Assign "User" role here

                    //After creating user, create 6 posts for them
                    var postFaker = new Faker<Post>()
                        .RuleFor(p => p.Title, f => f.Lorem.Sentence(6))
                        .RuleFor(p => p.Body, f => f.Lorem.Paragraphs(2))
                        .RuleFor(p => p.CreatedAt, f => f.Date.Past())
                        .RuleFor(p => p.Status, f => f.PickRandom<PostStatus>())
                        .RuleFor(p => p.IsDeleted, f => false)
                        .RuleFor(p => p.UserId, f => user.Id);

                    var posts = postFaker.Generate(6);
                    await context.Posts.AddRangeAsync(posts);
                }
            }
            // Hardcoded known test user
            var knownUserEmail = "testuser@example.com";
            var knownUser = new ApplicationUser
            {
                UserName = knownUserEmail,
                Email = knownUserEmail,
            };

            var existingUser = await userManager.FindByEmailAsync(knownUserEmail);
            if (existingUser == null)
            {
                var result = await userManager.CreateAsync(knownUser, "Test123!User");

                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(knownUser);
                    await userManager.ConfirmEmailAsync(knownUser, token);
                    await userManager.AddToRoleAsync(knownUser, "User");

                    // Add a couple of posts for this user if you want
                    var postFaker = new Faker<Post>()
                        .RuleFor(p => p.Title, f => f.Lorem.Sentence(6))
                        .RuleFor(p => p.Body, f => f.Lorem.Paragraphs(2))
                        .RuleFor(p => p.CreatedAt, f => f.Date.Past())
                        .RuleFor(p => p.LastModifiedAt, f => f.Date.Recent())
                        .RuleFor(p => p.Status, f => f.PickRandom<PostStatus>())
                        .RuleFor(p => p.IsDeleted, f => false)
                        .RuleFor(p => p.UserId, f => knownUser.Id);

                    var posts = postFaker.Generate(3);
                    await context.Posts.AddRangeAsync(posts);
                }
            }


            await context.SaveChangesAsync();

            // Seed Comments for Posts
            if (!await context.Comments.AnyAsync())
            {
                var allUsers = await context.Users.ToListAsync();
                var allPosts = await context.Posts.ToListAsync();

                var commentFaker = new Faker<Comment>()
                    .RuleFor(c => c.Content, f => f.Lorem.Sentences(f.Random.Int(1, 3)))
                    .RuleFor(c => c.CreatedAt, f => f.Date.Past(1))
                    .RuleFor(c => c.IsDeleted, f => false)
                    .RuleFor(c => c.UserId, f => f.PickRandom(allUsers).Id)
                    .RuleFor(c => c.PostId, f => f.PickRandom(allPosts).Id);

                var comments = commentFaker.Generate(60); // You can adjust count

                await context.Comments.AddRangeAsync(comments);
                await context.SaveChangesAsync();
            }

        }
    }
}
