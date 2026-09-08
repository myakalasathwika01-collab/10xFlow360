using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _10xFlow360.Models
{
    public class WorkflowCreateVM
    {
        [Required(ErrorMessage = "Workflow Name is required.")]
        [Display(Name = "Workflow Name")]
        public string WorkflowName { get; set; }

        [Required(ErrorMessage = "SAP Module is required.")]
        [Display(Name = "SAP Module")]
        public string SapModule { get; set; }

        [Required(ErrorMessage = "Company is required.")]
        [Display(Name = "Company")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Version is required.")]
        [Display(Name = "Version")]
        public string VersionNo { get; set; }

        // Workflow steps
        public List<WorkflowCreateStepVM> Steps { get; set; }

        // JSON sent from Create Workflow screen
        public string WorkflowStepsJson { get; set; }

        public WorkflowCreateVM()
        {
            Steps = new List<WorkflowCreateStepVM>();
        }
    }

    public class WorkflowCreateStepVM
    {
        public int StepNo { get; set; }

        public string StepName { get; set; }

        public string StepType { get; set; }

        public string TriggerType { get; set; }

        public string SapObject { get; set; }

        public string ConditionExpression { get; set; }

        public string NotificationType { get; set; }

        public string Recipient { get; set; }

        public string MessageTemplate { get; set; }

        public string WaitType { get; set; }

        public string FieldName { get; set; }

        public decimal? SlaHours { get; set; }
    }
}