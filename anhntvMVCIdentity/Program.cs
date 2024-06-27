using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using anhntvMVCIdentity.Data;
using anhntvMVCIdentity.Models.Entities;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using anhntvMVCIdentity.Models.Process;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
builder.Services.AddOptions();
var mailSettings = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSettings);
builder.Services.AddTransient<IEmailSender, SendMailService>();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();
   builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication().AddGoogle(googleOption =>
{
    googleOption.ClientId = Configuration["Authentication:Google:ClientId"];
    googleOption.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
});
 builder.Services.AddAuthorization(options =>
        {
            foreach (var permission in Enum.GetValues(typeof(SystemPermissions)).Cast<SystemPermissions>())
            {
                options.AddPolicy(permission.ToString(), policy =>
                    policy.RequireClaim("Permission", permission.ToString()));
            }
         
        });

builder.Services.AddTransient<EmployeeSeeder>();
builder.Services.Configure<IdentityOptions>(Options =>
{
Options.Lockout.DefaultLockoutTimeSpan =TimeSpan.FromMinutes(5);
Options.Lockout.MaxFailedAccessAttempts =5;
Options.Lockout.AllowedForNewUsers =true;
Options.Password.RequireDigit = true;
Options.Password.RequiredLength = 8;
Options.Password.RequireNonAlphanumeric = false;
Options.Password.RequireUppercase = true;
Options.Password.RequireLowercase = false;  
Options.SignIn.RequireConfirmedEmail = false;
Options.SignIn.RequireConfirmedPhoneNumber = false;
Options.User.RequireUniqueEmail = true;
});

builder.Services.AddDataProtection()
.PersistKeysToFileSystem(new DirectoryInfo(@"./keys"))
.SetApplicationName("YourAppname")
.SetDefaultKeyLifetime(TimeSpan.FromDays(14));
builder.Services.AddTransient<EmployeeSeeder>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
var app = builder.Build();
using (var scope = app.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var seeder = services.GetRequiredService<EmployeeSeeder>();
            seeder.SeedEmployees(100);
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
