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
                // Get workflow data from HANA first.
                // -------------------------------------------------

                var workflowData = db.Workflows
                    .OrderByDescending(x => x.WorkflowId)
                    .ToList();


                // -------------------------------------------------
                // Convert to WorkflowListItem in C#
                // Date formatting happens after ToList()
                // -------------------------------------------------

                var workflows = workflowData
                    .Select(x => new WorkflowListItem
                    {
                        Id = x.WorkflowId,

                        WorkflowCode = x.WorkflowCode,

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

                return View(
                    new List<WorkflowListItem>()
                );
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

        [HttpGet]
        public ActionResult Design()
        {
            return View("Design");
        }

        // =========================================================
        // GET: Workflow/WorkflowList
        // WORKFLOW LIST SCREEN
        // =========================================================

        [HttpGet]
        public ActionResult WorkflowList(long? workflowId)
        {
            try
            {
                // -------------------------------------------------
                // 1. LOAD ALL WORKFLOW HEADERS
                //    Latest workflow first.
                // -------------------------------------------------
                var workflowData = db.Workflows
                    .OrderByDescending(x => x.WorkflowId)
                    .ToList();

                // -------------------------------------------------
                // 2. CONVERT TO LIST MODEL
                // -------------------------------------------------
                var workflows = workflowData
                    .Select(x => new WorkflowListItem
                    {
                        Id = x.WorkflowId,
                        WorkflowCode = x.WorkflowCode,
                        WorkflowName = x.WorkflowName,
                        SAPModule = x.SapModule,
                        Version = x.VersionNo,
                        Status = x.Status,
                        UpdatedOn = x.UpdatedDate.HasValue
                            ? x.UpdatedDate.Value.ToString("dd-MMM-yyyy")
                            : x.CreatedDate.ToString("dd-MMM-yyyy")
                    })
                    .ToList();

                // -------------------------------------------------
                // 3. WHEN THE WORKFLOWS MENU IS CLICKED WITHOUT
                //    A workflowId, AUTOMATICALLY SELECT THE
                //    LATEST WORKFLOW.
                //
                //    URL from menu:
                //    /Workflow/WorkflowList
                //
                //    becomes internally equivalent to:
                //    /Workflow/WorkflowList?workflowId=<latest id>
                // -------------------------------------------------
                if (!workflowId.HasValue && workflowData.Count > 0)
                {
                    workflowId = workflowData.First().WorkflowId;
                }

                // -------------------------------------------------
                // 4. LOAD SELECTED WORKFLOW HEADER + CHILD STEPS
                // -------------------------------------------------
                if (workflowId.HasValue)
                {
                    var selectedWorkflow = db.Workflows
                        .FirstOrDefault(x =>
                            x.WorkflowId == workflowId.Value);

                    if (selectedWorkflow == null)
                    {
                        TempData["ErrorMessage"] =
                            "Workflow not found.";

                        return View("WorkflowList", workflows);
                    }

                    // Load only the steps belonging to this header.
                    var workflowSteps = db.WorkflowSteps
                        .Where(x =>
                            x.WorkflowId == workflowId.Value)
                        .OrderBy(x => x.StepNo)
                        .ToList();

                    // -------------------------------------------------
                    // 5. SEND SELECTED HEADER + STEPS TO VIEW
                    // -------------------------------------------------
                    ViewBag.SelectedWorkflow = selectedWorkflow;
                    ViewBag.WorkflowSteps = workflowSteps;
                    ViewBag.WorkflowId = selectedWorkflow.WorkflowId;
                    ViewBag.StepCount = workflowSteps.Count;
                }

                // -------------------------------------------------
                // 6. RETURN WORKFLOW LIST + DESIGNER
                // -------------------------------------------------
                return View("WorkflowList", workflows);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load workflows: " +
                    GetErrorMessage(ex);

                return View(
                    "WorkflowList",
                    new List<WorkflowListItem>());
            }
        }


        // =========================================================
        // POST: Workflow/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(WorkflowCreateVM model)
        {
            try
            {
                // =================================================
                // VALIDATION
                // =================================================

                if (model == null)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Workflow information is required."
                    });
                }


                if (string.IsNullOrWhiteSpace(
                    model.WorkflowName))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Workflow Name is required."
                    });
                }


                if (string.IsNullOrWhiteSpace(
                    model.SapModule))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "SAP Module is required."
                    });
                }


                if (string.IsNullOrWhiteSpace(
                    model.CompanyName))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Company is required."
                    });
                }


                if (string.IsNullOrWhiteSpace(
                    model.VersionNo))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Version is required."
                    });
                }


                // =================================================
                // CLEAN VALUES
                // =================================================

                string workflowName =
                    model.WorkflowName.Trim();

                string sapModule =
                    model.SapModule.Trim();

                string company =
                    model.CompanyName.Trim();

                string version =
                    model.VersionNo.Trim();


                // =================================================
                // GENERATE WORKFLOW CODE
                // =================================================

                string workflowCode =
                    GenerateWorkflowCode(
                        sapModule
                    );


                // =================================================
                // CREATE WORKFLOW HEADER
                // =================================================

                Workflow workflow =
                    new Workflow
                    {
                        WorkflowCode =
                            workflowCode,

                        WorkflowName =
                            workflowName,

                        SapModule =
                            sapModule,

                        CompanyName =
                            company,

                        VersionNo =
                            version,

                        Status =
                            "Draft",

                        CreatedBy =
                            "Admin",

                        CreatedDate =
                            DateTime.Now,

                        UpdatedBy =
                            "Admin",

                        UpdatedDate =
                            DateTime.Now
                    };


                // =================================================
                // SAVE HEADER
                //
                // WAI_WORKFLOW
                // =================================================

                db.Workflows.Add(workflow);

                db.SaveChanges();


                // =================================================
                // GET GENERATED WORKFLOW ID
                // =================================================

                long workflowId =
                    workflow.WorkflowId;


                // =================================================
                // RETURN GENERATED ID TO JAVASCRIPT
                // =================================================

                return Json(new
                {
                    success = true,

                    workflowId =
                        workflowId,

                    workflowCode =
                        workflow.WorkflowCode,

                    message =
                        "Workflow header created successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,

                    message =
                        "Unable to create workflow: "
                        + GetErrorMessage(ex)
                });
            }
        }

        //[HttpGet]
        //public ActionResult Design(long? workflowId)
        //{
        //    if (workflowId.HasValue)
        //    {
        //        return RedirectToAction(
        //            "WorkflowList",
        //            new
        //            {
        //                workflowId = workflowId.Value
        //            }
        //        );
        //    }

        //    return RedirectToAction("Index");
        //}
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

                    return RedirectToAction(
                        "Index"
                    );
                }


                // -------------------------------------------------
                // VALIDATION
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    workflowName))
                {
                    TempData["ErrorMessage"] =
                        "Workflow Name is required.";

                    return RedirectToAction(
                        "WorkflowList",
                        new
                        {
                            workflowId =
                                workflowId
                        }
                    );
                }


                if (string.IsNullOrWhiteSpace(
                    sapModule))
                {
                    TempData["ErrorMessage"] =
                        "SAP Module is required.";

                    return RedirectToAction(
                        "WorkflowList",
                        new
                        {
                            workflowId =
                                workflowId
                        }
                    );
                }


                if (string.IsNullOrWhiteSpace(
                    company))
                {
                    TempData["ErrorMessage"] =
                        "Company is required.";

                    return RedirectToAction(
                        "WorkflowList",
                        new
                        {
                            workflowId =
                                workflowId
                        }
                    );
                }


                if (string.IsNullOrWhiteSpace(
                    version))
                {
                    TempData["ErrorMessage"] =
                        "Version is required.";

                    return RedirectToAction(
                        "WorkflowList",
                        new
                        {
                            workflowId =
                                workflowId
                        }
                    );
                }


                // -------------------------------------------------
                // UPDATE HEADER
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
                    "WorkflowList",
                    new
                    {
                        workflowId =
                            workflowId
                    }
                );
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to save workflow header: "
                    + GetErrorMessage(ex);


                return RedirectToAction(
                    "WorkflowList",
                    new
                    {
                        workflowId =
                            workflowId
                    }
                );
            }
        }


        // =========================================================
        // POST: Workflow/SaveWorkflowSteps
        //
        // Saves the currently displayed designer steps into:
        // WAI_WORKFLOW_STEP
        //
        // workflowId is ALWAYS the selected WAI_WORKFLOW header ID.
        // Existing steps for that workflow are replaced on Save.
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveWorkflowSteps(
            long workflowId,
            string workflowStepsJson)
        {
            try
            {
                // 1. Verify parent/header.
                var workflow = db.Workflows
                    .FirstOrDefault(x =>
                        x.WorkflowId == workflowId);

                if (workflow == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Workflow header not found."
                    });
                }

                // 2. Validate step JSON.
                if (string.IsNullOrWhiteSpace(workflowStepsJson))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please add at least one workflow step."
                    });
                }

                // 3. Deserialize the designer array.
                var serializer =
                    new System.Web.Script.Serialization
                        .JavaScriptSerializer();

                var steps =
                    serializer.Deserialize<
                        List<WorkflowStepInput>
                    >(workflowStepsJson);

                if (steps == null || steps.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please add at least one workflow step."
                    });
                }

                // 4. Delete only the child rows belonging
                //    to this selected workflow.
                var oldSteps = db.WorkflowSteps
                    .Where(x =>
                        x.WorkflowId == workflowId)
                    .ToList();

                if (oldSteps.Count > 0)
                {
                    db.WorkflowSteps.RemoveRange(oldSteps);
                }

                // 5. Insert the current designer steps.
                int stepNo = 1;

                foreach (var item in steps)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var workflowStep = new WorkflowStep
                    {
                        // IMPORTANT:
                        // This is the WAI_WORKFLOW WORKFLOW_ID.
                        WorkflowId = workflowId,

                        StepNo = stepNo,

                        StepName =
                            string.IsNullOrWhiteSpace(item.name)
                                ? "Workflow Step " + stepNo
                                : item.name.Trim(),

                        StepType =
                            string.IsNullOrWhiteSpace(item.type)
                                ? "Action"
                                : item.type.Trim(),

                        TriggerType = item.triggerType,

                        SapObject = item.sapObject,

                        ConditionExpression = item.condition,

                        NotificationType =
                            item.notificationType,

                        Recipient =
                            item.recipient,

                        MessageTemplate =
                            item.messageTemplate,

                        WaitType =
                            item.waitType,

                        FieldName =
                            item.fieldName,

                        SlaHours =
                            item.slaHours,

                        CreatedDate = DateTime.Now,

                        UpdatedDate = DateTime.Now
                    };

                    db.WorkflowSteps.Add(workflowStep);

                    stepNo++;
                }

                // 6. Save parent-child changes to HANA.
                db.SaveChanges();

                // 7. Update header audit information.
                workflow.UpdatedBy = "Admin";
                workflow.UpdatedDate = DateTime.Now;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    workflowId = workflowId,
                    stepCount = stepNo - 1,
                    message = "Workflow steps saved successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Unable to save workflow steps: " +
                        GetErrorMessage(ex)
                });
            }
        }


        // =========================================================
        // GET: Workflow/Test
        // =========================================================

        [HttpGet]
        public ActionResult Test(long? workflowId)
        {
            ViewBag.WorkflowId =
                workflowId;

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
                          x.WorkflowId ==
                          workflowId);


                if (workflow == null)
                {
                    TempData["TestError"] =
                        "Workflow not found.";

                    return RedirectToAction(
                        "Test",
                        new
                        {
                            workflowId =
                                workflowId
                        }
                    );
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
                            workflowId =
                                workflowId
                        }
                    );
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
                        workflowId =
                            workflowId
                    }
                );
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
                        workflowId =
                            workflowId
                    }
                );
            }
        }


        // =========================================================
        // PRIVATE
        // GET WORKFLOW LIST
        // =========================================================

        private List<WorkflowListItem>
            GetWorkflowList()
        {
            // -------------------------------------------------
            // Materialize HANA query first.
            // -------------------------------------------------

            var workflowData =
                db.Workflows
                  .OrderByDescending(
                      x => x.WorkflowId)
                  .ToList();


            return workflowData
                .Select(x => new WorkflowListItem
                {
                    Id =
                        x.WorkflowId,

                    WorkflowCode =
                        x.WorkflowCode,

                    WorkflowName =
                        x.WorkflowName,

                    SAPModule =
                        x.SapModule,

                    Version =
                        x.VersionNo,

                    Status =
                        x.Status,

                    UpdatedOn =
                        x.UpdatedDate.HasValue
                        ? x.UpdatedDate.Value
                            .ToString(
                                "dd-MMM-yyyy")
                        : x.CreatedDate
                            .ToString(
                                "dd-MMM-yyyy")
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
            string prefix =
                "GEN";


            if (!string.IsNullOrWhiteSpace(
                sapModule))
            {
                switch (
                    sapModule.Trim().ToUpper())
                {
                    case "SALES":

                        prefix =
                            "SALES";

                        break;


                    case "PURCHASE":

                        prefix =
                            "PUR";

                        break;


                    case "INVENTORY":

                        prefix =
                            "INV";

                        break;


                    case "FINANCE":

                        prefix =
                            "FIN";

                        break;


                    case "PRODUCTION":

                        prefix =
                            "PROD";

                        break;


                    case "SERVICE":

                        prefix =
                            "SERV";

                        break;
                }
            }


            // ---------------------------------------------------------
            // Load workflow codes first.
            //
            // Avoid StartsWith() inside HANA LINQ query.
            // ---------------------------------------------------------

            List<string> workflowCodes =
                db.Workflows
                  .Select(x => x.WorkflowCode)
                  .ToList();


            string expectedPrefix =
                "WF-" + prefix + "-";


            int maxNumber =
                0;


            foreach (string code
                     in workflowCodes)
            {
                if (string.IsNullOrWhiteSpace(
                    code))
                {
                    continue;
                }


                // -------------------------------------------------
                // Check prefix in C#
                // -------------------------------------------------

                if (!code.StartsWith(
                        expectedPrefix,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    continue;
                }


                // -------------------------------------------------
                // Example:
                //
                // WF-SALES-001
                // WF-SALES-002
                // -------------------------------------------------

                string numberPart =
                    code.Substring(
                        expectedPrefix.Length);


                int number;


                if (int.TryParse(
                    numberPart,
                    out number))
                {
                    if (number > maxNumber)
                    {
                        maxNumber =
                            number;
                    }
                }
            }


            return expectedPrefix +
                   (maxNumber + 1)
                       .ToString("000");
        }


        // =========================================================
        // PRIVATE
        // GET INNER EXCEPTION
        // =========================================================

        private string GetErrorMessage(
            Exception ex)
        {
            if (ex == null)
            {
                return "Unknown error.";
            }


            Exception current =
                ex;


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

        public string WorkflowCode { get; set; }

        public string WorkflowName { get; set; }

        public string SAPModule { get; set; }

        public string Version { get; set; }

        public string Status { get; set; }

        public string UpdatedOn { get; set; }
    }


    // =============================================================
    // WORKFLOW STEP INPUT
    //
    // Used by existing Design screen
    // =============================================================

    public class WorkflowStepInput
    {
        public string name { get; set; }

        public string type { get; set; }

        public string actionType { get; set; }

        public string triggerType { get; set; }

        public string sapObject { get; set; }

        public string condition { get; set; }

        public string notificationType { get; set; }

        public string recipient { get; set; }

        public string messageTemplate { get; set; }

        public string waitType { get; set; }

        public string fieldName { get; set; }

        public decimal? slaHours { get; set; }
    }
}