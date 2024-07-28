using Portal.Models.Ticket;

namespace Portal.data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
            
        }

        public DbSet<ApplicationUser> ApplicationUser { get; set; } = default!;
        public DbSet<Ticket> Tickets { get; set; } = default!;

        public DbSet<Upload> Uploads { get; set; } = default!;

        public DbSet<Log> Logs { get; set; } = default!;


    }


}
