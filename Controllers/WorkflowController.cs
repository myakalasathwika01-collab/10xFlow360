using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _10xFlow360.Controllers
{
    public class WorkflowController : Controller
    {
        // =====================================================
        // GET: Workflow/WorkflowList
        // =====================================================

        [HttpGet]
        public ActionResult WorkflowList()
        {
            var workflows = new List<WorkflowListItem>
            {
                new WorkflowListItem
                {
                    Id = 1,
                    WorkflowName = "Sales Order to Invoice Follow-up",
                    SAPModule = "Sales",
                    Version = "V1.0",
                    Status = "Draft",
                    UpdatedOn = "03-Sep-2026"
                },

                new WorkflowListItem
                {
                    Id = 2,
                    WorkflowName = "Purchase Order Approval",
                    SAPModule = "Purchase",
                    Version = "V1.0",
                    Status = "Published",
                    UpdatedOn = "02-Sep-2026"
                },

                new WorkflowListItem
                {
                    Id = 3,
                    WorkflowName = "Stock Reorder Process",
                    SAPModule = "Inventory",
                    Version = "V1.0",
                    Status = "Draft",
                    UpdatedOn = "01-Sep-2026"
                }
            };

            return View(workflows);
        }


        // =====================================================
        // GET: Workflow/NewWorkflow
        // NEW WORKFLOW DESIGNER
        // =====================================================

        [HttpGet]
        public ActionResult NewWorkflow()
        {
            return View();
        }


        // =====================================================
        // GET: Workflow/Design
        // EXISTING DESIGN SCREEN
        // DON'T CHANGE
        // =====================================================

        [HttpGet]
        public ActionResult Design()
        {
            return View();
        }


        // =====================================================
        // POST: Workflow/SaveDraft
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDraft(
            string workflowName,
            string sapModule,
            string company,
            string version)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workflowName))
                {
                    TempData["ErrorMessage"] =
                        "Workflow name is required.";

                    return RedirectToAction("Design");
                }

                TempData["SuccessMessage"] =
                    "Workflow draft saved successfully.";

                return RedirectToAction("Design");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to save workflow draft: " +
                    ex.Message;

                return RedirectToAction("Design");
            }
        }


        // =====================================================
        // GET: Workflow/Test
        // =====================================================

        [HttpGet]
        public ActionResult Test()
        {
            return View();
        }


        // =====================================================
        // POST: Workflow/RunTest
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunTest(
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

                    return RedirectToAction("Test");
                }

                TempData["TestMessage"] =
                    "Workflow test completed successfully.";

                return RedirectToAction("Test");
            }
            catch (Exception ex)
            {
                TempData["TestError"] =
                    ex.Message;

                return RedirectToAction("Test");
            }
        }
    }


    // =========================================================
    // WORKFLOW LIST ITEM
    // =========================================================

    public class WorkflowListItem
    {
        public int Id { get; set; }

        public string WorkflowName { get; set; }

        public string SAPModule { get; set; }

        public string Version { get; set; }

        public string Status { get; set; }

        public string UpdatedOn { get; set; }
    }
}