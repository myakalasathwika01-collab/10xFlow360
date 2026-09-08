using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _10xFlow360.Models
{
    [Table("WAI_WORKFLOW_STEP")]
    public class WorkflowStep
    {
        [Key]
        [Column("STEP_ID")]
        public long StepId { get; set; }

        [Required]
        [Column("WORKFLOW_ID")]
        public long WorkflowId { get; set; }

        [Required]
        [Column("STEP_NO")]
        public int StepNo { get; set; }

        [Required]
        [StringLength(200)]
        [Column("STEP_NAME")]
        public string StepName { get; set; }

        [Required]
        [StringLength(50)]
        [Column("STEP_TYPE")]
        public string StepType { get; set; }

        [StringLength(100)]
        [Column("TRIGGER_TYPE")]
        public string TriggerType { get; set; }

        [StringLength(100)]
        [Column("SAP_OBJECT")]
        public string SapObject { get; set; }

        [StringLength(2000)]
        [Column("CONDITION_EXPRESSION")]
        public string ConditionExpression { get; set; }

        [StringLength(100)]
        [Column("NOTIFICATION_TYPE")]
        public string NotificationType { get; set; }

        [StringLength(200)]
        [Column("RECIPIENT")]
        public string Recipient { get; set; }

        [StringLength(4000)]
        [Column("MESSAGE_TEMPLATE")]
        public string MessageTemplate { get; set; }

        [StringLength(100)]
        [Column("WAIT_TYPE")]
        public string WaitType { get; set; }

        [StringLength(200)]
        [Column("FIELD_NAME")]
        public string FieldName { get; set; }

        [Column("SLA_HOURS")]
        public decimal? SlaHours { get; set; }

        [Column("CREATED_DATE")]
        public DateTime CreatedDate { get; set; }

        [Column("UPDATED_DATE")]
        public DateTime? UpdatedDate { get; set; }
    }
}