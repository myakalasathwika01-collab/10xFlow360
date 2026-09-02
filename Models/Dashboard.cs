using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _10xFlow360.Models
{
    public class Dashboard
    {
        public int ActiveWorkflows { get; set; }

        public int CurrentlyProcessing { get; set; }

        public int RunningInstances { get; set; }

        public int CompletedToday { get; set; }

        public int CompletedPercentage { get; set; }

        public int Overdue { get; set; }

        public int SlaCompliance { get; set; }

        public int CompletedOnTime { get; set; }

        public int Escalated { get; set; }

        public List<ActiveProcess> ActiveProcesses { get; set; }
    }


    public class ActiveProcess
    {
        public string Workflow { get; set; }

        public string WorkflowCode { get; set; }

        public string Module { get; set; }

        public int Running { get; set; }

        public int Completed { get; set; }

        public string Status { get; set; }
    }
}