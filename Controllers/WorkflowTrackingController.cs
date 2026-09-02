using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _10xFlow360.Controllers
{
    public class WorkflowTrackingController : Controller
    {
        // GET: WorkflowTracking
        public ActionResult Index(string instanceId = "WF-10021")
        {
            var instances = GetWorkflowInstances();

            var selectedInstance = instances
                .FirstOrDefault(x => x.InstanceId == instanceId);

            if (selectedInstance == null)
            {
                selectedInstance = instances.First();
            }

            var model = new WorkflowTrackingViewModel
            {
                TotalInstances = 186,
                InProgress = 42,
                Completed = 138,
                Overdue = 6,

                Instances = instances,

                SelectedInstance = selectedInstance,

                Timeline = GetTimeline()
            };

            return View(model);
        }


        // POST: WorkflowTracking/Refresh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Refresh()
        {
            TempData["TrackingMessage"] =
                "Process tracking information refreshed successfully.";

            return RedirectToAction("Index");
        }


        // POST: WorkflowTracking/Escalate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Escalate(string instanceId)
        {
            TempData["TrackingMessage"] =
                "Workflow instance " + instanceId + " has been escalated.";

            return RedirectToAction("Index", new
            {
                instanceId = instanceId
            });
        }


        // =========================================================
        // SAMPLE WORKFLOW INSTANCES
        // =========================================================

        private List<WorkflowInstanceViewModel> GetWorkflowInstances()
        {
            return new List<WorkflowInstanceViewModel>
            {
                new WorkflowInstanceViewModel
                {
                    InstanceId = "WF-10021",
                    Document = "SO 10345",
                    Customer = "ABC Trading",
                    CurrentStage = "Wait for Delivery Date",
                    Owner = "Delivery Team",
                    Elapsed = "01:15 Hr",
                    Status = "Pending",
                    Workflow = "Sales Order to Invoice",
                    SapDocument = "Sales Order 10345",
                    CurrentOwner = "Delivery Team",
                    SlaElapsed = "1 Hr 15 Min",
                    SlaLimit = "2 Hr Limit",
                    SlaPercentage = 63
                },

                new WorkflowInstanceViewModel
                {
                    InstanceId = "WF-10022",
                    Document = "SO 10346",
                    Customer = "XYZ LLC",
                    CurrentStage = "Wait for Invoice",
                    Owner = "Accounts",
                    Elapsed = "03:20 Hr",
                    Status = "Running",
                    Workflow = "Sales Order to Invoice",
                    SapDocument = "Sales Order 10346",
                    CurrentOwner = "Accounts",
                    SlaElapsed = "1 Hr 10 Min",
                    SlaLimit = "4 Hr Limit",
                    SlaPercentage = 29
                },

                new WorkflowInstanceViewModel
                {
                    InstanceId = "WF-10023",
                    Document = "SO 10347",
                    Customer = "Falcon Trading",
                    CurrentStage = "Delivery Planning",
                    Owner = "Logistics",
                    Elapsed = "05:42 Hr",
                    Status = "Overdue",
                    Workflow = "Sales Order to Invoice",
                    SapDocument = "Sales Order 10347",
                    CurrentOwner = "Logistics",
                    SlaElapsed = "5 Hr 42 Min",
                    SlaLimit = "4 Hr Limit",
                    SlaPercentage = 100
                },

                new WorkflowInstanceViewModel
                {
                    InstanceId = "WF-10024",
                    Document = "SO 10340",
                    Customer = "Delta LLC",
                    CurrentStage = "Completed",
                    Owner = "-",
                    Elapsed = "04:31 Hr",
                    Status = "Completed",
                    Workflow = "Sales Order to Invoice",
                    SapDocument = "Sales Order 10340",
                    CurrentOwner = "-",
                    SlaElapsed = "Completed",
                    SlaLimit = "4 Hr Limit",
                    SlaPercentage = 72
                }
            };
        }


        // =========================================================
        // PROCESS TIMELINE
        // =========================================================

        private List<TimelineViewModel> GetTimeline()
        {
            return new List<TimelineViewModel>
            {
                new TimelineViewModel
                {
                    Title = "Sales Order Created",
                    Time = "10:01 AM",
                    Description = "SO 10345 created by John",
                    IsCurrent = false
                },

                new TimelineViewModel
                {
                    Title = "Workflow Started",
                    Time = "10:02 AM",
                    Description = "Sales Order to Invoice workflow initiated",
                    IsCurrent = false
                },

                new TimelineViewModel
                {
                    Title = "Delivery Team Notified",
                    Time = "10:02 AM",
                    Description = "Test notification successfully generated",
                    IsCurrent = false
                },

                new TimelineViewModel
                {
                    Title = "Waiting for Delivery Date",
                    Time = "Current Stage",
                    Description = "Waiting for Delivery Team action",
                    IsCurrent = true
                }
            };
        }
    }


    // =============================================================
    // MAIN VIEW MODEL
    // =============================================================

    public class WorkflowTrackingViewModel
    {
        public int TotalInstances { get; set; }

        public int InProgress { get; set; }

        public int Completed { get; set; }

        public int Overdue { get; set; }

        public List<WorkflowInstanceViewModel> Instances { get; set; }

        public WorkflowInstanceViewModel SelectedInstance { get; set; }

        public List<TimelineViewModel> Timeline { get; set; }
    }


    // =============================================================
    // WORKFLOW INSTANCE
    // =============================================================

    public class WorkflowInstanceViewModel
    {
        public string InstanceId { get; set; }

        public string Document { get; set; }

        public string Customer { get; set; }

        public string CurrentStage { get; set; }

        public string Owner { get; set; }

        public string Elapsed { get; set; }

        public string Status { get; set; }

        public string Workflow { get; set; }

        public string SapDocument { get; set; }

        public string CurrentOwner { get; set; }

        public string SlaElapsed { get; set; }

        public string SlaLimit { get; set; }

        public int SlaPercentage { get; set; }
    }


    // =============================================================
    // TIMELINE
    // =============================================================

    public class TimelineViewModel
    {
        public string Title { get; set; }

        public string Time { get; set; }

        public string Description { get; set; }

        public bool IsCurrent { get; set; }
    }
}