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
    }
}