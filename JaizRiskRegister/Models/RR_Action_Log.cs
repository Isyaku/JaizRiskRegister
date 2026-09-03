namespace JaizRiskRegister.Models
{
    public class RR_Action_Log
    {
        public int Id { get; set; }
        public string? StaffID { get; set; }
        public DateTime ActionDate { get; set; }
        public string? Action { get; set; }
    }
}