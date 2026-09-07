using System.Data.Entity;
using _10xFlow360.Models;

namespace _10xFlow360.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("name=DBEntities")
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public virtual DbSet<Workflow> Workflows { get; set; }
    }
}