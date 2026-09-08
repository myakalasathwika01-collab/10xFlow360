using System.Configuration;
using System.Data.Entity;
using Sap.Data.Hana;
using _10xFlow360.Models;

namespace _10xFlow360.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base(CreateConnection(), true)
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        private static HanaConnection CreateConnection()
        {
            string connectionString =
                ConfigurationManager.AppSettings["ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new System.Exception(
                    "ConnectionString is missing from Web.config appSettings."
                );
            }

            return new HanaConnection(connectionString);
        }

        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    }
}