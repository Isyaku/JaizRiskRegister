using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace JaizRiskRegister.Models
{
    public class RR_Submission
    {
        [Key]
        public int ID { get; set; }
        public string Risk_ID { get; set; }
        public int Submission_ID { get; set; }
        public int Department_ID { get; set; }
        public string? Submission_Type { get; set; }
        public int? Submission_Month { get; set; }
        public int? Submission_Quarter { get; set; }
        public int? Submission_Year { get; set; }

        [Display(Name = "Impact_1_2_3_4_or_5")]
        public int? Impact { get; set; }

        [Display(Name = "Likelihood_1_2_3_4_or_5")]
        public int? Likelihood { get; set; }

        [Display(Name = "Inherent Risk level(Impact x Likelihood)")]
        public int? Inherent_level { get; set; }        

        [Display(Name = "Control_score_1_2_3_4_or_5")]
        public int? Control_Score { get; set; }
        public string? Status { get; set; }
        public int? Residual_Score { get; set; }
        public string? Risk_Level { get; set; }        
        public int Risk_id_GT { get; set; }
        public int? SubStatus { get; set; }
        public int? ApprovalStage { get; set; }
        public int? Modified { get; set; }
        public string? EditedBy { get; set; }
    }
}
