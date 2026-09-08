using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xFlow360.Models
{
    [Table("WAI_WORKFLOW", Schema = "ZLEVERIWMS")]
    public class Workflow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("WORKFLOW_ID")]
        public long WorkflowId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("WORKFLOW_CODE")]
        public string WorkflowCode { get; set; }

        [Required]
        [StringLength(200)]
        [Column("WORKFLOW_NAME")]
        public string WorkflowName { get; set; }

        [StringLength(100)]
        [Column("SAP_MODULE")]
        public string SapModule { get; set; }

        [StringLength(200)]
        [Column("COMPANY_NAME")]
        public string CompanyName { get; set; }

        [StringLength(20)]
        [Column("VERSION_NO")]
        public string VersionNo { get; set; }

        [StringLength(30)]
        [Column("STATUS")]
        public string Status { get; set; }

        [StringLength(100)]
        [Column("CREATED_BY")]
        public string CreatedBy { get; set; }

        [Column("CREATED_DATE")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("UPDATED_BY")]
        public string UpdatedBy { get; set; }

        [Column("UPDATED_DATE")]
        public DateTime? UpdatedDate { get; set; }
    }
}