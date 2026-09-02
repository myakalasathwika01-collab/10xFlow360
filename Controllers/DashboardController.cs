using _10xFlow360.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _10xFlow360.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            var model = new Dashboard
            {
                ActiveWorkflows = 8,

                CurrentlyProcessing = 6,

                RunningInstances = 42,

                CompletedToday = 31,

                CompletedPercentage = 12,

                Overdue = 6,

                SlaCompliance = 91,

                CompletedOnTime = 87,

                Escalated = 8,

                ActiveProcesses = new List<ActiveProcess>
                {
                    new ActiveProcess
                    {
                        Workflow = "Sales Order to Invoice",
                        WorkflowCode = "WF-SALES-001",
                        Module = "Sales",
                        Running = 18,
                        Completed = 126,
                        Status = "Active"
                    },

                    new ActiveProcess
                    {
                        Workflow = "Purchase Request Approval",
                        WorkflowCode = "WF-PUR-001",
                        Module = "Purchase",
                        Running = 11,
                        Completed = 82,
                        Status = "Active"
                    },

                    new ActiveProcess
                    {
                        Workflow = "Customer Collection Follow-up",
                        WorkflowCode = "WF-FIN-002",
                        Module = "Finance",
                        Running = 13,
                        Completed = 54,
                        Status = "Active"
                    }
                }
            };

            return View(model);
        }
    }
}