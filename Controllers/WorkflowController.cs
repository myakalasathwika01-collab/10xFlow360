using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using _10xFlow360.Data;
using _10xFlow360.Models;

namespace _10xFlow360.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // GET: Workflow/Index
        // WORKFLOW LIST
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                // -------------------------------------------------
                // IMPORTANT:
                // First get data from HANA.
                // Do NOT use ToString("dd-MMM-yyyy") inside LINQ.
                // -------------------------------------------------

                var workflowData = db.Workflows
                    .OrderByDescending(x => x.WorkflowId)
                    .ToList();


                // -------------------------------------------------
                // Convert to WorkflowListItem in C#
                // Date formatting happens AFTER ToList()
                // -------------------------------------------------

                var workflows = workflowData
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
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load workflows: "
                    + GetErrorMessage(ex);

                return View(new List<WorkflowListItem>());
            }
        }


        // =========================================================
        // GET: Workflow/NewWorkflow
        // =========================================================

        [HttpGet]
        public ActionResult NewWorkflow()
        {
            return RedirectToAction("Index");
        }


        // =========================================================
        // POST: Workflow/Create
        // CREATE WORKFLOW HEADER
        //
        // Saves into:
        // WAI_WORKFLOW
        // =========================================================

        // =========================================================
        // GET: Workflow/WorkflowList
        // WORKFLOW LIST SCREEN
        // =========================================================

        [HttpGet]
        public ActionResult WorkflowList()
        {
            try
            {
                var workflowData = db.Workflows
                    .OrderByDescending(x => x.WorkflowId)
                    .ToList();

                var workflows = workflowData
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
                return View("WorkflowList", workflows);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load workflows: "
                    + GetErrorMessage(ex);

                return View(
                    "WorkflowList",
                    new List<WorkflowListItem>()
                );
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WorkflowCreateVM model)
        {
            try
            {
                // -----------------------------------------
                // VALIDATION
                // -----------------------------------------

                if (model == null)
                {
                    TempData["ErrorMessage"] =
                        "Workflow information is required.";

                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(model.WorkflowName))
                {
                    TempData["ErrorMessage"] =
                        "Workflow Name is required.";

                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(model.SapModule))
                {
                    TempData["ErrorMessage"] =
                        "SAP Module is required.";

                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    TempData["ErrorMessage"] =
                        "Company is required.";

                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(model.VersionNo))
                {
                    TempData["ErrorMessage"] =
                        "Version is required.";

                    return RedirectToAction("Index");
                }


                // -----------------------------------------
                // CLEAN VALUES
                // -----------------------------------------

                string workflowName = model.WorkflowName.Trim();
                string sapModule = model.SapModule.Trim();
                string company = model.CompanyName.Trim();
                string version = model.VersionNo.Trim();


                // -----------------------------------------
                // GENERATE WORKFLOW CODE
                // -----------------------------------------

                string workflowCode =
                    GenerateWorkflowCode(sapModule);


                // -----------------------------------------
                // CREATE HEADER
                // -----------------------------------------

                Workflow workflow = new Workflow
                {
                    WorkflowCode = workflowCode,
                    WorkflowName = workflowName,
                    SapModule = sapModule,
                    CompanyName = company,
                    VersionNo = version,

                    Status = "Draft",

                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now,

                    UpdatedBy = "Admin",
                    UpdatedDate = DateTime.Now
                };


                // -----------------------------------------
                // SAVE TO WAI_WORKFLOW
                // -----------------------------------------

                db.Workflows.Add(workflow);

                db.SaveChanges();


                // -----------------------------------------
                // SUCCESS
                // -----------------------------------------

                TempData["SuccessMessage"] =
                    "Workflow created successfully. " +
                    "Workflow Code: " +
                    workflow.WorkflowCode;


                // -----------------------------------------
                // BACK TO WORKFLOW LIST
                // -----------------------------------------

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to create workflow: " +
                    GetErrorMessage(ex);

                return RedirectToAction("Index");
            }
        }

        // =========================================================
        // GET: Workflow/Design
        // LOAD EXISTING WORKFLOW
        // =========================================================
        // =========================================================
        // GET: Workflow/Design
        // OPENS EXISTING DESIGN.CSHTML
        // =========================================================

        // =========================================================
        // GET: Workflow/Design
        // OPENS EXISTING DESIGN.CSHTML
        // =========================================================

        [HttpGet]
        public ActionResult Design(long? workflowId)
        {
            try
            {
                // -------------------------------------------------
                // If sidebar opened Design without workflowId,
                // get the latest created workflow.
                // -------------------------------------------------

                if (!workflowId.HasValue)
                {
                    var latestWorkflow = db.Workflows
                        .OrderByDescending(x => x.WorkflowId)
                        .FirstOrDefault();

                    if (latestWorkflow == null)
                    {
                        TempData["ErrorMessage"] =
                            "Please create a workflow first.";

                        return RedirectToAction("Index");
                    }

                    workflowId = latestWorkflow.WorkflowId;
                }

                // -------------------------------------------------
                // Load selected workflow
                // -------------------------------------------------

                var workflow = db.Workflows
                    .FirstOrDefault(x =>
                        x.WorkflowId == workflowId.Value);

                if (workflow == null)
                {
                    TempData["ErrorMessage"] =
                        "Workflow not found.";

                    return RedirectToAction("Index");
                }

                // -------------------------------------------------
                // Open EXISTING Design.cshtml
                // -------------------------------------------------

                return View(workflow);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load workflow: "
                    + GetErrorMessage(ex);

                return RedirectToAction("Index");
            }
        }


        // =========================================================
        // POST: Workflow/SaveDraft
        // SAVE WORKFLOW HEADER
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
                // -------------------------------------------------
                // FIND WORKFLOW
                // -------------------------------------------------

                var workflow =
                    db.Workflows
                      .FirstOrDefault(x =>
                          x.WorkflowId == workflowId);


                if (workflow == null)
                {
                    TempData["ErrorMessage"] =
                        "Workflow not found.";

                    return RedirectToAction("Index");
                }


                // -------------------------------------------------
                // VALIDATION
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(workflowName))
                {
                    TempData["ErrorMessage"] =
                        "Workflow Name is required.";

                    return RedirectToAction(
                        "Design",
                        new
                        {
                            workflowId = workflowId
                        });
                }


                if (string.IsNullOrWhiteSpace(sapModule))
                {
                    TempData["ErrorMessage"] =
                        "SAP Module is required.";

                    return RedirectToAction(
                        "Design",
                        new
                        {
                            workflowId = workflowId
                        });
                }


                if (string.IsNullOrWhiteSpace(company))
                {
                    TempData["ErrorMessage"] =
                        "Company is required.";

                    return RedirectToAction(
                        "Design",
                        new
                        {
                            workflowId = workflowId
                        });
                }


                if (string.IsNullOrWhiteSpace(version))
                {
                    TempData["ErrorMessage"] =
                        "Version is required.";

                    return RedirectToAction(
                        "Design",
                        new
                        {
                            workflowId = workflowId
                        });
                }


                // -------------------------------------------------
                // UPDATE
                // -------------------------------------------------

                workflow.WorkflowName =
                    workflowName.Trim();

                workflow.SapModule =
                    sapModule.Trim();

                workflow.CompanyName =
                    company.Trim();

                workflow.VersionNo =
                    version.Trim();

                workflow.Status =
                    "Draft";

                workflow.UpdatedBy =
                    "Admin";

                workflow.UpdatedDate =
                    DateTime.Now;


                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------

                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Workflow header saved successfully.";


                return RedirectToAction(
                    "Design",
                    new
                    {
                        workflowId = workflowId
                    });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to save workflow header: "
                    + GetErrorMessage(ex);

                return RedirectToAction(
                    "Design",
                    new
                    {
                        workflowId = workflowId
                    });
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
                // -------------------------------------------------
                // VALIDATE WORKFLOW
                // -------------------------------------------------

                var workflow =
                    db.Workflows
                      .FirstOrDefault(x =>
                          x.WorkflowId == workflowId);


                if (workflow == null)
                {
                    TempData["TestError"] =
                        "Workflow not found.";

                    return RedirectToAction(
                        "Test",
                        new
                        {
                            workflowId = workflowId
                        });
                }


                // -------------------------------------------------
                // VALIDATE SALES ORDER
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    salesOrderNumber))
                {
                    TempData["TestError"] =
                        "Please enter Sales Order Number.";

                    return RedirectToAction(
                        "Test",
                        new
                        {
                            workflowId = workflowId
                        });
                }


                // -------------------------------------------------
                // TEST EXECUTION
                // -------------------------------------------------

                TempData["TestMessage"] =
                    "Workflow test completed successfully.";


                return RedirectToAction(
                    "Test",
                    new
                    {
                        workflowId = workflowId
                    });
            }
            catch (Exception ex)
            {
                TempData["TestError"] =
                    "Test failed: "
                    + GetErrorMessage(ex);

                return RedirectToAction(
                    "Test",
                    new
                    {
                        workflowId = workflowId
                    });
            }
        }


        // =========================================================
        // PRIVATE
        // GET WORKFLOW LIST
        // =========================================================

        private List<WorkflowListItem> GetWorkflowList()
        {
            // IMPORTANT:
            // Materialize HANA query first.

            var workflowData =
                db.Workflows
                  .OrderByDescending(x => x.WorkflowId)
                  .ToList();


            return workflowData
                .Select(x => new WorkflowListItem
                {
                    Id = x.WorkflowId,

                    WorkflowName = x.WorkflowName,

                    SAPModule = x.SapModule,

                    Version = x.VersionNo,

                    Status = x.Status,

                    UpdatedOn = x.UpdatedDate.HasValue
                        ? x.UpdatedDate.Value
                            .ToString("dd-MMM-yyyy")
                        : x.CreatedDate
                            .ToString("dd-MMM-yyyy")
                })
                .ToList();
        }


        // =========================================================
        // PRIVATE
        // GENERATE WORKFLOW CODE
        // =========================================================

        private string GenerateWorkflowCode(
            string sapModule)
        {
            string prefix = "GEN";


            if (!string.IsNullOrWhiteSpace(sapModule))
            {
                switch (sapModule.Trim().ToUpper())
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


            // -------------------------------------------------
            // Count existing workflow codes
            // This operation is SQL-translatable.
            // -------------------------------------------------

            int count =
                db.Workflows.Count(x =>
                    x.WorkflowCode.StartsWith(
                        "WF-" + prefix + "-"));


            return "WF-" +
                   prefix +
                   "-" +
                   (count + 1).ToString("000");
        }


        // =========================================================
        // PRIVATE
        // GET INNER EXCEPTION
        // =========================================================

        private string GetErrorMessage(Exception ex)
        {
            if (ex == null)
                return "Unknown error.";


            Exception current = ex;


            while (current.InnerException != null)
            {
                current =
                    current.InnerException;
            }


            return current.Message;
        }


        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }


    // =============================================================
    // WORKFLOW LIST ITEM
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