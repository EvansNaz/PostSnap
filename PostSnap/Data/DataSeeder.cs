using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PostSnap.Models;

namespace PostSnap.Data
{
    public class DataSeeder
    {
        public static async Task SeedUserAndPosts(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager)
        {
            //Skip seeding if users already exist
            if(await context.Users.AnyAsync())
                return;

            //Create 5 fake users
            var userFaker = new Faker<ApplicationUser>()
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email());

            var users = userFaker.Generate(5);

            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, "Password123!");
                if(result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    await userManager.ConfirmEmailAsync(user, token);//Confirm Email
                    //After creating user, create 6 posts for them
                    var postFaker = new Faker<Post>()
                        .RuleFor(p => p.Title, f => f.Lorem.Sentence(6))
                        .RuleFor(p => p.Body, f => f.Lorem.Paragraphs(2))
                        .RuleFor(p => p.CreatedAt, f => f.Date.Past())
                        .RuleFor(p => p.LastModifiedAt, f => f.Date.Recent())
                        .RuleFor(p => p.Status, f => f.PickRandom<PostStatus>())
                        .RuleFor(p => p.IsDeleted, f => false)
                        .RuleFor(p => p.UserId, f => user.Id);

                    var posts = postFaker.Generate(6);
                    await context.Posts.AddRangeAsync(posts);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
