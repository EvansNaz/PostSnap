using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using PostSnap.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --------------------------------------------
// SEED FAKE USERS AND POSTS (Only runs once)
// --------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Access required services from the container
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Call the method that seeds 5 users, each with 6 posts
    await DataSeeder.SeedUserAndPosts(context, userManager);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Posts}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

/*
 Pagination � break the post listing into pages so it�s not overwhelming. (DONE!!!!!!!!!)

Search & Filter � add the ability to search posts by title, body, or user.(DONE!!!!!!!!!)

Sorting � allow sorting by date, title, or status.

Authorization Checks � make sure users can only edit/delete their posts.

Improve the UI � make the views cleaner (maybe use cards, bootstrap, etc.).

Comments or Likes � add extra features to your post model.

Unit Tests / Validation � add tests or deeper validation logic.

File Uploads � attach images to posts?

!!!! user can't delete his own posts or comments just his account but his things he posted stay with no owner

users:
hont@gmail.com
jkA12!sa

john@gmail.com

 */