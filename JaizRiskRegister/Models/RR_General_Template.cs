using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace JaizRiskRegister.Models
{
    public class RR_General_Template
    {
        [Key]
        public long ID { get; set; }
        public string Risk_ID { get; set; }
        public int Department_ID { get; set; }
        public string? Type_of_Risk { get; set; }
        public string? Risk_Area { get; set; }
        public string? Risk_Description { get; set; }
        public string? Business_Unit_Impacted { get; set; }        
        public int? Impact_1_2_3_4_or_5 { get; set; }
        public int? Likelihood_1_2_3_4_or_5 { get; set; }

        [Display(Name = "Inherent Risk level(Impact x Likelihood)")]
        public int? Inherent_level { get; set; }
        public string? Cause_of_Risk { get; set; }
        public string? Risk_Impact { get; set; }
        public string? Primary_Owner { get; set; }

        [Display(Name = "Key controls assessed by Management (preliminary assessment)")]
        public string? Management_Controls { get; set; }

        [Display(Name = "Control_score_1_2_3_4_or_5")]
        public string? Control_Score { get; set; }
        public string? Status { get; set; }
        public string? Action_Plan { get; set; }
        public string? Assigned_To { get; set; }
        public decimal? Residual_Score { get; set; }
        public string? Risk_Quadrant { get; set; }
        public string? Risk_Level { get; set; }
        public int? Modified { get; set; }
        public int? IsApproved { get; set; }
        public string? EditedBy { get; set; }

    }
}
