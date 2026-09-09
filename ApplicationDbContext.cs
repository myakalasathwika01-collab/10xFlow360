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

        // =========================================================
        // DBSETS
        // =========================================================

        public DbSet<Workflow> Workflows { get; set; }

        public DbSet<WorkflowStep> WorkflowSteps { get; set; }


        // =========================================================
        // HANA TABLE MAPPING
        // =========================================================

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------------------------------
            // WORKFLOW HEADER
            // ZLEVERIWMS.WAI_WORKFLOW
            // -----------------------------------------------------

            modelBuilder.Entity<Workflow>()
                .ToTable(
                    "WAI_WORKFLOW",
                    "ZLEVERIWMS"
                );


            // -----------------------------------------------------
            // WORKFLOW STEPS
            // ZLEVERIWMS.WAI_WORKFLOW_STEP
            // -----------------------------------------------------

            modelBuilder.Entity<WorkflowStep>()
                .ToTable(
                    "WAI_WORKFLOW_STEP",
                    "ZLEVERIWMS"
                );
        }
    }
}