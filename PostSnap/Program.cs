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
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --------------------------------------------
// SEED FAKE USERS AND POSTS WITH COMMENTS  & ADMIN (ONLY SEEDS ONCE)
// --------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Access required services from the container
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    //Create roles (admin, user)
    await RoleSeeder.SeedRoles(roleManager);

    // Call the method that seeds 5 users, each with 6 posts
    await UserSeeder.SeedUserAndPosts(context, userManager);
    
    //Create Admin
    await AdminSeeder.SeedAdmin(userManager, roleManager);

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
Ready Users when project is seeded:
----------------------------------------------------------------------

USERNAME:
testuser@example.com

PASSWORD:
Test123!User
---------------------------------------------------------------------

The rest of the seeded Users have the PASSWORD: 
Password123!
----------------------------------------------------------------------

ADMIN'S ACCOUNT:

USERNAME:
admin@example.com

PASSWORD:
AdminPassword123!
 */