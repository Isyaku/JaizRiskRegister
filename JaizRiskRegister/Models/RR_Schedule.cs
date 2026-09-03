using System.ComponentModel.DataAnnotations;

namespace JaizRiskRegister.Models
{
    public class RR_Schedule
    {
        [Key]
        public int Submission_ID { get; set; }
        [Required(ErrorMessage = "Submission type is required.")]
        public string Submission_Type { get; set; }
        public int? Submission_Month { get; set; }  
        public int? Submission_Quarter { get; set; }
        [Required(ErrorMessage = "Year is required.")]
        public int? Submission_Year { get; set; }
        public string? Status { get; set; }
        public string? Created_By { get; set; }
    }
}
