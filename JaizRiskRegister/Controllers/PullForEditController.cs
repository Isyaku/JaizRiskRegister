using JaizRiskRegister.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class PullForEditController : Controller
    {
        private readonly RiskDbContext _contextRisk;
        Helpers.Utility util = new Helpers.Utility();
        public PullForEditController(RiskDbContext contextRisk)
        {
            _contextRisk = contextRisk;
        }

        public IActionResult Index()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            var departments = _contextRisk.departments.ToList();
            ViewBag.DepartmentList = new SelectList(departments, "id", "department");
            return View();
        }

        [HttpGet]
        public IActionResult DepartmentalRisk(int departmentId, int submissionMonth, int submissionQuarter, int submissionYear, string submissionType)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                if ((submissionType.ToLower() == "monthly") && (submissionMonth > 0 && submissionMonth <= 12))
                {
                    var departmentalRisks = (from JaizRisks in _contextRisk.RR_General_Template
                                             join DepartmentalRiskSubmission in _contextRisk.RR_Submission
                                             on JaizRisks.Risk_ID equals DepartmentalRiskSubmission.Risk_ID
                                             where DepartmentalRiskSubmission.Department_ID == departmentId &&
                                             DepartmentalRiskSubmission.Submission_Month == submissionMonth &&
                                             DepartmentalRiskSubmission.Submission_Year == submissionYear &&
                                             DepartmentalRiskSubmission.Submission_Type == submissionType &&
                                             DepartmentalRiskSubmission.ApprovalStage != 2
                                             select new
                                             {
                                                 DepartmentalRiskSubmission.Submission_ID,
                                                 JaizRisks.Risk_ID,
                                                 JaizRisks.Department_ID,
                                                 DepartmentalRiskSubmission.Submission_Type,
                                                 JaizRisks.Type_of_Risk,
                                                 JaizRisks.Risk_Area,
                                                 JaizRisks.Risk_Description,
                                                 JaizRisks.Business_Unit_Impacted,
                                                 DepartmentalRiskSubmission.Impact,
                                                 DepartmentalRiskSubmission.Likelihood,
                                                 DepartmentalRiskSubmission.Inherent_level,
                                                 JaizRisks.Management_Controls,
                                                 DepartmentalRiskSubmission.Status,
                                                 DepartmentalRiskSubmission.Control_Score,
                                                 JaizRisks.Action_Plan,
                                                 JaizRisks.Assigned_To,
                                                 DepartmentalRiskSubmission.Residual_Score,
                                                 DepartmentalRiskSubmission.Risk_Level,
                                                 DepartmentalRiskSubmission.Modified,
                                             }).ToList();
                    return Json(departmentalRisks);

                }
                else if ((submissionType.ToLower() == "quarterly") && (submissionQuarter > 0 && submissionQuarter <= 4))
                {
                    var departmentalRisks = (from JaizRisks in _contextRisk.RR_General_Template
                                             join DepartmentalRiskSubmission in _contextRisk.RR_Submission
                                             on JaizRisks.Risk_ID equals DepartmentalRiskSubmission.Risk_ID
                                             where DepartmentalRiskSubmission.Department_ID == departmentId &&
                                             DepartmentalRiskSubmission.Submission_Quarter == submissionQuarter &&
                                             DepartmentalRiskSubmission.Submission_Year == submissionYear &&
                                             DepartmentalRiskSubmission.Submission_Type == submissionType &&
                                             DepartmentalRiskSubmission.ApprovalStage != 2
                                             select new
                                             {
                                                 DepartmentalRiskSubmission.Submission_ID,
                                                 JaizRisks.Risk_ID,
                                                 JaizRisks.Department_ID,
                                                 DepartmentalRiskSubmission.Submission_Type,
                                                 JaizRisks.Type_of_Risk,
                                                 JaizRisks.Risk_Area,
                                                 JaizRisks.Risk_Description,
                                                 JaizRisks.Business_Unit_Impacted,
                                                 DepartmentalRiskSubmission.Impact,
                                                 DepartmentalRiskSubmission.Likelihood,
                                                 DepartmentalRiskSubmission.Inherent_level,
                                                 JaizRisks.Management_Controls,
                                                 DepartmentalRiskSubmission.Status,
                                                 DepartmentalRiskSubmission.Control_Score,
                                                 JaizRisks.Action_Plan,
                                                 JaizRisks.Assigned_To,
                                                 DepartmentalRiskSubmission.Residual_Score,
                                                 DepartmentalRiskSubmission.Risk_Level,
                                                 DepartmentalRiskSubmission.Modified,
                                             }).ToList();

                    return Json(departmentalRisks);
                }
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error in DepartmentalRisk method :::: {ex.Message}");
            }
            return Json(new { success = false, message = "Unable to fetch records." });
        }

        [HttpPost]
        public IActionResult UpdateRisk([FromBody] List<RiskUpdateModel> updatedRisks)
        {
            try
            {
                var user = HttpContext.Session.GetString("user");

                if (!SetSessionData())
                {
                    return RedirectToAction("Login", "Home");
                }

                foreach (var riskUpdate in updatedRisks)
                {
                    var risk = _contextRisk.RR_Submission.FirstOrDefault
                    (
                        r => r.Submission_ID == riskUpdate.Submission_ID &&
                        r.Risk_ID == riskUpdate.Risk_ID &&
                        r.Department_ID == riskUpdate.Department_ID &&
                        r.Submission_Type == riskUpdate.Submission_Type
                    );

                    if (risk != null)
                    {
                        risk.Impact = riskUpdate.Impact;
                        risk.Likelihood = riskUpdate.Likelihood;
                        risk.Inherent_level = riskUpdate.Inherent_level;
                        risk.Control_Score = riskUpdate.Control_Score;
                        risk.Status = riskUpdate.Status;
                        risk.Residual_Score = riskUpdate.Residual_Score;
                        risk.Risk_Level = riskUpdate.Risk_Level;
                        risk.SubStatus = 0;
                        risk.ApprovalStage = 1;
                        risk.Modified = riskUpdate.Modified;
                        risk.EditedBy = user;

                        _contextRisk.Update(risk);
                        _contextRisk.SaveChanges();
                    }
                }

                string dept = _contextRisk.departments.Where(a => a.id == updatedRisks[0].Department_ID).FirstOrDefault().department;

                var actionLog = new RR_Action_Log
                {
                    StaffID = user,
                    ActionDate = DateTime.Now,
                    Action = $"Updated submitted risk scores for deparment,  {dept}",
                };
                _contextRisk.RR_Action_Log.Add(actionLog);
                _contextRisk.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error in UpdateRisk method :::: {ex.Message}");
            }
            return BadRequest();
        }

        private bool SetSessionData()
        {
            try
            {
                var user = HttpContext.Session.GetString("user");
                if (user == null)
                {
                    return false;
                }

                ViewBag.User = user;
                ViewBag.RMDApprover = HttpContext.Session.GetString("RRegisterApprover");
                ViewBag.LoginRole = HttpContext.Session.GetString("LoginRole");

                return true;
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error setting user session data on PullForEditController :::: {ex.Message}");
            }
            return false;

        }
        public class RiskUpdateModel
        {
            public int Submission_ID { get; set; }
            public string Risk_ID { get; set; }
            public int Department_ID { get; set; }
            public string Submission_Type { get; set; }
            public int Impact { get; set; }
            public int Likelihood { get; set; }
            public int Inherent_level { get; set; }
            public int Control_Score { get; set; }
            public string Status { get; set; }
            public int Residual_Score { get; set; }
            public string Risk_Level { get; set; }
            public int Modified { get; set; }
        }

    }
}
