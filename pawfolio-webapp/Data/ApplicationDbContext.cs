using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using pawfolio_webapp.Models;

namespace pawfolio_webapp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
    {
        // Allow the application to use the Pet model in the database context
        public DbSet<Models.Pet> Pets { get; set; }
        public DbSet<pawfolio_webapp.Models.FileModel> Files => Set<FileModel>();
        public DbSet<VetInfo> VetInfos => Set<VetInfo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VetInfo>()
                .HasOne(vetInfo => vetInfo.User)
                .WithOne(user => user.VetInfo)
                .HasForeignKey<VetInfo>(vetInfo => vetInfo.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
