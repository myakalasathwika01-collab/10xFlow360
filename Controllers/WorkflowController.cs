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


                return View(
                    "WorkflowList",
                    workflows
                );
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


        // =========================================================
        // POST: Workflow/Create
        //
        // STEP 1:
        // CREATE WORKFLOW HEADER ONLY
        //
        // Saves into:
        // WAI_WORKFLOW
        //
        // After saving, returns:
        // WORKFLOW_ID
        //
        // Steps are NOT saved here.
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


        // =========================================================
        // POST: Workflow/SaveCreateWorkflowSteps
        //
        // STEP 2:
        // SAVE STEPS CREATED FROM WORKFLOW LIST
        //
        // Parent:
        // WAI_WORKFLOW
        //
        // Child:
        // WAI_WORKFLOW_STEP
        //
        // IMPORTANT:
        // workflowId comes from the header created by Create()
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveCreateWorkflowSteps(
            long workflowId,
            string workflowStepsJson)
        {
            try
            {
                // =================================================
                // CHECK WORKFLOW HEADER
                // =================================================

                var workflow =
                    db.Workflows
                      .FirstOrDefault(x =>
                          x.WorkflowId == workflowId);


                if (workflow == null)
                {
                    return Json(new
                    {
                        success = false,

                        message =
                            "Workflow not found."
                    });
                }


                // =================================================
                // CHECK JSON
                // =================================================

                if (string.IsNullOrWhiteSpace(
                    workflowStepsJson))
                {
                    return Json(new
                    {
                        success = false,

                        message =
                            "Please add at least one workflow step."
                    });
                }


                // =================================================
                // DESERIALIZE JSON
                // =================================================

                var serializer =
                    new System.Web.Script.Serialization
                        .JavaScriptSerializer();


                List<WorkflowCreateStepVM> steps =
                    serializer.Deserialize<
                        List<WorkflowCreateStepVM>
                    >(
                        workflowStepsJson
                    );


                if (steps == null ||
                    steps.Count == 0)
                {
                    return Json(new
                    {
                        success = false,

                        message =
                            "Please add at least one workflow step."
                    });
                }


                // =================================================
                // REMOVE OLD STEPS
                //
                // This is useful if the user saves the workflow
                // again and changes the step list.
                // =================================================

                var oldSteps =
                    db.WorkflowSteps
                      .Where(x =>
                          x.WorkflowId == workflowId)
                      .ToList();


                if (oldSteps.Count > 0)
                {
                    db.WorkflowSteps.RemoveRange(
                        oldSteps
                    );
                }


                // =================================================
                // INSERT CURRENT STEPS
                // =================================================

                int stepNo = 1;


                foreach (var item in steps)
                {
                    if (item == null)
                    {
                        continue;
                    }


                    WorkflowStep workflowStep =
                        new WorkflowStep
                        {
                            // =====================================
                            // PARENT WORKFLOW ID
                            // =====================================

                            WorkflowId =
                                workflowId,


                            // =====================================
                            // STEP NUMBER
                            // =====================================

                            StepNo =
                                stepNo,


                            // =====================================
                            // STEP NAME
                            // =====================================

                            StepName =
                                string.IsNullOrWhiteSpace(
                                    item.StepName)
                                ? "Workflow Step " + stepNo
                                : item.StepName.Trim(),


                            // =====================================
                            // STEP TYPE
                            // =====================================

                            StepType =
                                string.IsNullOrWhiteSpace(
                                    item.StepType)
                                ? "Action"
                                : item.StepType.Trim(),


                            // =====================================
                            // TRIGGER TYPE
                            // =====================================

                            TriggerType =
                                item.TriggerType,


                            // =====================================
                            // SAP OBJECT
                            // =====================================

                            SapObject =
                                item.SapObject,


                            // =====================================
                            // CONDITION
                            // =====================================

                            ConditionExpression =
                                item.ConditionExpression,


                            // =====================================
                            // NOTIFICATION
                            // =====================================

                            NotificationType =
                                item.NotificationType,

                            Recipient =
                                item.Recipient,

                            MessageTemplate =
                                item.MessageTemplate,


                            // =====================================
                            // WAIT
                            // =====================================

                            WaitType =
                                item.WaitType,


                            // =====================================
                            // FIELD
                            // =====================================

                            FieldName =
                                item.FieldName,


                            // =====================================
                            // SLA
                            // =====================================

                            SlaHours =
                                item.SlaHours,


                            // =====================================
                            // AUDIT
                            // =====================================

                            CreatedDate =
                                DateTime.Now,

                            UpdatedDate =
                                DateTime.Now
                        };


                    // =============================================
                    // ADD CHILD RECORD
                    // =============================================

                    db.WorkflowSteps.Add(
                        workflowStep
                    );


                    stepNo++;
                }


                // =================================================
                // SAVE ALL STEPS
                // =================================================

                db.SaveChanges();


                // =================================================
                // SUCCESS
                // =================================================

                return Json(new
                {
                    success = true,

                    workflowId =
                        workflowId,

                    message =
                        "Workflow and steps saved successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,

                    message =
                        "Unable to save workflow steps: "
                        + GetErrorMessage(ex)
                });
            }
        }


        // =========================================================
        // GET: Workflow/Design
        //
        // EXISTING DESIGN SCREEN
        //
        // This is kept because your project already has a Design
        // screen and existing functionality.
        //
        // The NEW workflow creation flow does NOT need to redirect
        // here.
        // =========================================================

        [HttpGet]
        public ActionResult Design(long? workflowId)
        {
            try
            {
                // -------------------------------------------------
                // If Design is opened without workflowId,
                // get latest workflow.
                // -------------------------------------------------

                if (!workflowId.HasValue)
                {
                    var latestWorkflow =
                        db.Workflows
                          .OrderByDescending(
                              x => x.WorkflowId)
                          .FirstOrDefault();


                    if (latestWorkflow == null)
                    {
                        TempData["ErrorMessage"] =
                            "Please create a workflow first.";

                        return RedirectToAction(
                            "Index"
                        );
                    }


                    workflowId =
                        latestWorkflow.WorkflowId;
                }


                // -------------------------------------------------
                // Load selected workflow
                // -------------------------------------------------

                var workflow =
                    db.Workflows
                      .FirstOrDefault(x =>
                          x.WorkflowId ==
                          workflowId.Value);


                if (workflow == null)
                {
                    TempData["ErrorMessage"] =
                        "Workflow not found.";

                    return RedirectToAction(
                        "Index"
                    );
                }


                // -------------------------------------------------
                // Open existing Design.cshtml
                // -------------------------------------------------

                return View(workflow);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load workflow: "
                    + GetErrorMessage(ex);

                return RedirectToAction(
                    "Index"
                );
            }
        }


        // =========================================================
        // POST: Workflow/SaveDraft
        //
        // EXISTING DESIGN HEADER UPDATE
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
                        "Design",
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
                        "Design",
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
                        "Design",
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
                        "Design",
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
                    "Design",
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
                    "Design",
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
        // EXISTING DESIGN SCREEN STEP SAVE
        //
        // This remains available for your existing Design screen.
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveWorkflowSteps(
            long workflowId,
            string workflowStepsJson)
        {
            try
            {
                // -------------------------------------------------
                // CHECK WORKFLOW HEADER
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
                // CHECK JSON
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    workflowStepsJson))
                {
                    TempData["ErrorMessage"] =
                        "Please add at least one workflow step.";

                    return RedirectToAction(
                        "Design",
                        new
                        {
                            workflowId =
                                workflowId
                        }
                    );
                }


                // -------------------------------------------------
                // DESERIALIZE
                // -------------------------------------------------

                var serializer =
                    new System.Web.Script.Serialization
                        .JavaScriptSerializer();


                var steps =
                    serializer.Deserialize<
                        List<WorkflowStepInput>
                    >(
                        workflowStepsJson
                    );


                if (steps == null ||
                    steps.Count == 0)
                {
                    TempData["ErrorMessage"] =
                        "Please add at least one workflow step.";

                    return RedirectToAction(
                        "Design",
                        new
                        {
                            workflowId =
                                workflowId
                        }
                    );
                }


                // -------------------------------------------------
                // REMOVE OLD STEPS
                // -------------------------------------------------

                var oldSteps =
                    db.WorkflowSteps
                      .Where(x =>
                          x.WorkflowId ==
                          workflowId)
                      .ToList();


                if (oldSteps.Count > 0)
                {
                    db.WorkflowSteps.RemoveRange(
                        oldSteps
                    );
                }


                // -------------------------------------------------
                // INSERT CURRENT STEPS
                // -------------------------------------------------

                int stepNo = 1;


                foreach (var item in steps)
                {
                    if (item == null)
                    {
                        continue;
                    }


                    var workflowStep =
                        new WorkflowStep
                        {
                            WorkflowId =
                                workflowId,

                            StepNo =
                                stepNo,

                            StepName =
                                string.IsNullOrWhiteSpace(
                                    item.name)
                                ? "Workflow Step " + stepNo
                                : item.name.Trim(),

                            StepType =
                                string.IsNullOrWhiteSpace(
                                    item.type)
                                ? "Action"
                                : item.type.Trim(),

                            TriggerType =
                                item.triggerType,

                            SapObject =
                                item.sapObject,

                            ConditionExpression =
                                item.condition,

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

                            CreatedDate =
                                DateTime.Now,

                            UpdatedDate =
                                DateTime.Now
                        };


                    db.WorkflowSteps.Add(
                        workflowStep
                    );


                    stepNo++;
                }


                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------

                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Workflow steps saved successfully.";


                return RedirectToAction(
                    "Design",
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
                    "Unable to save workflow steps: "
                    + GetErrorMessage(ex);


                return RedirectToAction(
                    "Design",
                    new
                    {
                        workflowId =
                            workflowId
                    }
                );
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