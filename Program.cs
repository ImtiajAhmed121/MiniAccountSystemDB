using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniAccountSystemDB.Data;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;

namespace MiniAccountSystemDB
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Developer Exception Page for EF
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Connection String
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Register EF Core & Identity
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages();

            var app = builder.Build();

            //Creating roles on startup
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string[] roles = { "Admin", "Accountant", "Viewer" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                var user = await userManager.FindByEmailAsync("A@gmail.com"); // using actual email
                if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            // Only ONE app.Run()!
            await app.RunAsync();
        }
    }
}
