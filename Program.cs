using Becas.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Repositorio en memoria respaldado por Session
builder.Services.AddScoped<Becas.Data.ISolicitudBecaRepository, Becas.Data.SessionSolicitudBecaRepository>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddDefaultTokenProviders();

builder.Services.AddSingleton<InMemoryUserStore>();
builder.Services.AddSingleton<InMemoryRoleStore>();
builder.Services.AddSingleton<IUserStore<IdentityUser>>(sp => sp.GetRequiredService<InMemoryUserStore>());
builder.Services.AddSingleton<IRoleStore<IdentityRole>>(sp => sp.GetRequiredService<InMemoryRoleStore>());

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<DummyIdentitySeeder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DummyIdentitySeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "Becas",
    pattern: "Beca/{action}/{id?}",
    defaults: new { controller = "Beca", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
