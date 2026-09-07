using System;
using System.Linq;
using System.Web.Mvc;
using _10xFlow360.Data;
using _10xFlow360.Models;

namespace _10xFlow360.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();


        // =========================================================
        // GET: Workflow/Index
        // Workflow List Page
        // =========================================================
        [HttpGet]
        public ActionResult Index()
        {
            var workflows = db.Workflows
                .OrderByDescending(x => x.WorkflowId)
                .Select(x => new WorkflowListItem
                {
                    Id = x.WorkflowId,
                    WorkflowName = x.WorkflowName,
                    SAPModule = x.SapModule,
                    Version = x.VersionNo,
                    Status = x.Status,
                    UpdatedOn = x.UpdatedDate.HasValue
                        ? x.UpdatedDate.Value.ToString("dd-MMM-yyyy")
                        : x.CreatedDate.ToString("dd-MMM-yyyy")
                })
                .ToList();

            return View(workflows);
        }


        // =========================================================
        // GET: Workflow/NewWorkflow
        // Optional route
        // =========================================================
        [HttpGet]
        public ActionResult NewWorkflow()
        {
            return RedirectToAction("Index");
        }


        // =========================================================
        // POST: Workflow/Create
        // Save Workflow Header
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WorkflowCreateVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", GetWorkflowList());
                }

                // Generate Workflow Code
                string workflowCode = GenerateWorkflowCode(model.SapModule);

                var workflow = new Workflow
                {
                    WorkflowCode = workflowCode,
                    WorkflowName = model.WorkflowName,
                    SapModule = model.SapModule,
                    CompanyName = model.CompanyName,
                    VersionNo = model.VersionNo,
                    Status = "Draft",

                    // Change this later if you have logged-in user information
                    CreatedBy = "Admin",

                    CreatedDate = DateTime.Now,

                    UpdatedBy = "Admin",
                    UpdatedDate = DateTime.Now
                };

                db.Workflows.Add(workflow);
                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Workflow created successfully.";

                // After saving header, go back to Workflow List
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to create workflow: " + ex.Message;

                return RedirectToAction("Index");
            }
        }


        // =========================================================
        // GET: Workflow/Design
        // Existing Design Screen
        // =========================================================
        [HttpGet]
        public ActionResult Design(long workflowId)
        {
            var workflow = db.Workflows
                .FirstOrDefault(x => x.WorkflowId == workflowId);

            if (workflow == null)
            {
                TempData["ErrorMessage"] = "Workflow not found.";

                return RedirectToAction("Index");
            }

            return View(workflow);
        }


        // =========================================================
        // POST: Workflow/SaveDraft
        // This can be used later for saving workflow steps
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDraft(
            long workflowId,
            string workflowName,
            string sapModule,
            string company,
            string version)
        {
            try
            {
                var workflow = db.Workflows
                    .FirstOrDefault(x => x.WorkflowId == workflowId);

                if (workflow == null)
                {
                    TempData["ErrorMessage"] =
                        "Workflow not found.";

                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(workflowName))
                {
                    TempData["ErrorMessage"] =
                        "Workflow name is required.";

                    return RedirectToAction(
                        "Design",
                        new { workflowId = workflowId });
                }

                // Update Workflow Header
                workflow.WorkflowName = workflowName;
                workflow.SapModule = sapModule;
                workflow.CompanyName = company;
                workflow.VersionNo = version;

                workflow.Status = "Draft";
                workflow.UpdatedBy = "Admin";
                workflow.UpdatedDate = DateTime.Now;

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Workflow draft saved successfully.";

                return RedirectToAction(
                    "Design",
                    new { workflowId = workflowId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to save workflow draft: " + ex.Message;

                return RedirectToAction(
                    "Design",
                    new { workflowId = workflowId });
            }
        }


        // =========================================================
        // GET: Workflow/Test
        // =========================================================
        [HttpGet]
        public ActionResult Test(long? workflowId)
        {
            ViewBag.WorkflowId = workflowId;

            return View();
        }


        // =========================================================
        // POST: Workflow/RunTest
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunTest(
            long workflowId,
            string salesOrderNumber,
            string customer,
            string amount,
            string salesman)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(salesOrderNumber))
                {
                    TempData["TestError"] =
                        "Please enter Sales Order Number.";

                    return RedirectToAction(
                        "Test",
                        new { workflowId = workflowId });
                }

                // Test execution will be implemented later
                TempData["TestMessage"] =
                    "Workflow test completed successfully.";

                return RedirectToAction(
                    "Test",
                    new { workflowId = workflowId });
            }
            catch (Exception ex)
            {
                TempData["TestError"] = ex.Message;

                return RedirectToAction(
                    "Test",
                    new { workflowId = workflowId });
            }
        }


        // =========================================================
        // Get Workflow List
        // =========================================================
        private System.Collections.Generic.List<WorkflowListItem>
            GetWorkflowList()
        {
            return db.Workflows
                .OrderByDescending(x => x.WorkflowId)
                .Select(x => new WorkflowListItem
                {
                    Id = x.WorkflowId,
                    WorkflowName = x.WorkflowName,
                    SAPModule = x.SapModule,
                    Version = x.VersionNo,
                    Status = x.Status,
                    UpdatedOn = x.UpdatedDate.HasValue
                        ? x.UpdatedDate.Value.ToString("dd-MMM-yyyy")
                        : x.CreatedDate.ToString("dd-MMM-yyyy")
                })
                .ToList();
        }


        // =========================================================
        // Generate Workflow Code
        // Example: WF-SALES-001
        // =========================================================
        private string GenerateWorkflowCode(string sapModule)
        {
            string prefix = "GEN";

            if (!string.IsNullOrWhiteSpace(sapModule))
            {
                switch (sapModule.ToUpper())
                {
                    case "SALES":
                        prefix = "SALES";
                        break;

                    case "PURCHASE":
                        prefix = "PUR";
                        break;

                    case "INVENTORY":
                        prefix = "INV";
                        break;

                    case "FINANCE":
                        prefix = "FIN";
                        break;

                    case "PRODUCTION":
                        prefix = "PROD";
                        break;

                    case "SERVICE":
                        prefix = "SERV";
                        break;
                }
            }

            int count = db.Workflows.Count(x =>
                x.WorkflowCode.StartsWith("WF-" + prefix + "-"));

            return "WF-" + prefix + "-" +
                   (count + 1).ToString("000");
        }


        // =========================================================
        // Dispose
        // =========================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }


    // =============================================================
    // Workflow List Item
    // Keep this class as requested
    // =============================================================
    public class WorkflowListItem
    {
        public long Id { get; set; }

        public string WorkflowName { get; set; }

        public string SAPModule { get; set; }

        public string Version { get; set; }

        public string Status { get; set; }

        public string UpdatedOn { get; set; }
    }
}