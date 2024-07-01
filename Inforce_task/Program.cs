using Inforce_task.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Налаштування служб
builder.Services.AddDbContext<DB_Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DB_Context>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

// Налаштування середовища виконання
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();

    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
    // specifying the Swagger JSON endpoint.
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

static async Task CreateRoles(IServiceProvider serviceProvider, IConfiguration configuration)
{
    // Initializing custom roles
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    string[] roleNames = { "Admin", "User" };
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Create the roles and seed them to the database
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Creating a super user who could maintain the web app
    var userEmail = configuration.GetSection("AppSettings")["UserEmail"];
    var userPassword = configuration.GetSection("AppSettings")["UserPassword"];

    if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userPassword))
    {
        throw new ArgumentNullException("UserEmail or UserPassword is not set in appsettings.json");
    }

    var powerUser = new User
    {
        UserName = userEmail,
        Email = userEmail
    };

    var _user = await userManager.FindByEmailAsync(userEmail);

    if (_user == null)
    {
        var createPowerUser = await userManager.CreateAsync(powerUser, userPassword);
        if (createPowerUser.Succeeded)
        {
            // here we tie the new user to the role
            await userManager.AddToRoleAsync(powerUser, "Admin");
        }
    }
}


// Виклик CreateRoles перед запуском додатку
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var configuration = services.GetRequiredService<IConfiguration>();

    try
    {
        await CreateRoles(services, configuration);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred while creating roles.");
    }
}

app.Run();
