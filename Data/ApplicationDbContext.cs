using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MiniAccountSystemDB.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer("Server=DESKTOP-KQGG6BD\\SQLEXPRESS;Database=MiniAccountSystemDB;Trusted_Connection=True;MultipleActiveResultSets=true")
        .Options)
        {
        }
    }
}
