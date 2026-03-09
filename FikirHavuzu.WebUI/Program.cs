using FikirHavuzu.Business.Abstract;
using FikirHavuzu.Business.Concrete;
using FikirHavuzu.DataAccess;
using FikirHavuzu.DataAccess.Abstract;
using FikirHavuzu.DataAccess.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using FikirHavuzu.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IUserService, UserManager>();

builder.Services.AddScoped<IIdeaRepository, EfIdeaRepository>();
builder.Services.AddScoped<IIdeaService, IdeaManager>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/Index"; 
    options.LoginPath = "/Account/Login";
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        var admin = new User
        {
            FirstName = "Sistem",
            LastName = "Yöneticisi",
            Email = "admin@fikirhavuzu.com",
            Password = "123", 
            Role = "Admin",
            IsActive = true,
            PhoneNumber = "0000000000", 
            TCIdentityNumber = "11111111111",
            RegistrationNumber="111111",
            TotalScore = 0
        };
        context.Users.Add(admin);
        context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
