using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace pawfolio_webapp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        // Allow the application to use the Pet model in the database context
        public DbSet<Models.Pet> Pets { get; set; }
    }
}
