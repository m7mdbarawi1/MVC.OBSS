using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) DbContext (reads "DefaultConnection" from appsettings.json)
var cs = builder.Configuration.GetConnectionString("OBSS") ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<OBSSContext>(opt => opt.UseSqlServer(cs));

// 2) Cookie Authentication (single scheme)
builder.Services.AddAuthentication("OBSSAuth")
    .AddCookie("OBSSAuth", options =>
    { 
        options.Cookie.Name = "OBSS.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3) Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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
    pattern: "{controller=Home}/{action=welcome}/{id?}"
);

app.Run();
