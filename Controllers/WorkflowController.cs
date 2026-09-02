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
        // GET: Workflow/Design
        // =====================================================

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
            // Save logic can be added later.

            TempData["SuccessMessage"] =
                "Workflow draft saved successfully.";

            return RedirectToAction("Design");
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
            // Actual SAP workflow test logic
            // can be added here later.

            TempData["TestMessage"] =
                "Workflow test completed successfully.";

            return RedirectToAction("Test");
        }
    }
}