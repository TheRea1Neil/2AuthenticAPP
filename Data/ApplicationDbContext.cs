using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _2AuthenticAPP.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string> // Ensure string is specified as the TKey type here
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicitly configure the type of the primary key for the AspNetRoles table
            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(r => r.Id).ValueGeneratedOnAdd().HasColumnType("nvarchar(450)"); // If you're using GUIDs as strings
            });

            // If you have a custom role class replace 'IdentityRole' with your custom class

            // Do the same for the AspNetUsers table if necessary
            builder.Entity<IdentityUser>(entity =>
            {
                entity.Property(u => u.Id).ValueGeneratedOnAdd().HasColumnType("nvarchar(450)"); // If you're using GUIDs as strings
            });

            // If you have a custom user class replace 'IdentityUser' with your custom class
        }
    }
}
