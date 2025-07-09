using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Helper;
using WebApp.Models;
using WebApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // User settings
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign in settings
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<TicketRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<FileRepository>();
builder.Services.AddScoped<TicketHistoryRepository>();
builder.Services.AddScoped<TicketCommentsRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<ThemeFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ThemeFilter>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //db.Database.EnsureDeleted();
    //db.Database.EnsureCreated();
    // ^^^^^^  Development only !!
    db.Database.Migrate();
}

await EnsureIdentity.SeedDefaultAccounts(app);
DbInitializer.SeedDb(app);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;

    if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/images") || path.StartsWith("/lib"))
    {
        await next();
        return;
    }

    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    await next();
});

var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
Directory.CreateDirectory(uploadPath);

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=DefaultPage}/{id?}");

app.Run();
