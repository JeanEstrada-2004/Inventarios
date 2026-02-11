using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Inventarios.Data; // Namespace donde estará tu DbContext

var builder = WebApplication.CreateBuilder(args);

// =======================
// CONFIGURACIÓN DE BD
// =======================
var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// =======================
// IDENTITY (ROLES + USERS)
// =======================
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    // Permitir espacios en el nombre de usuario
    options.User.AllowedUserNameCharacters += " ";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =======================
// PIPELINE HTTP
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // IMPORTANTE
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
