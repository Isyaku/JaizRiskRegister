using JaizRiskRegister.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class RiskEntryController : Controller
    {
        private readonly RiskDbContext _contextRisk;
        Helpers.Utility util = new Helpers.Utility();
        public RiskEntryController(RiskDbContext contextRisk)
        {
            _contextRisk = contextRisk;
        }
        public IActionResult Index()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            //var currentYear = DateTime.Now.Year;
            var currentUser = HttpContext.Session.GetString("user");
            //var currentOpenedSchedule = _contextRisk.RR_Schedule.FirstOrDefault(a => a.Submission_Year == currentYear && a.Status == "Open");
            var currentOpenedSchedule = _contextRisk.RR_Schedule.FirstOrDefault(a => a.Status == "Open");

            if (currentOpenedSchedule == null)
            {
                ViewBag.ErrMessage = "Risk Register entry is closed!";
                return View();
            }

            var departments = _contextRisk.departments.Where(d => d.head == currentUser).ToList();
            ViewBag.DepartmentList = new SelectList(departments, "id", "department");
            return View();
        }

        [HttpGet]
        public ActionResult DepartmentalRisk(int departmentId)
        {
            List<RR_General_Template> departmentalRisks = new List<RR_General_Template>();
            try
            {
                departmentalRisks = _contextRisk.RR_General_Template.Where(a => a.Department_ID == departmentId && a.IsApproved == 1).ToList();
            }
            catch (Exception ex) { }

            return Json(departmentalRisks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<RR_Submission> risksRecords)
        {
            var user = HttpContext.Session.GetString("user");
            var quarter = 0;

            var quarters = new Dictionary<int, int>
            {
                { 1, 3 },
                { 2, 6 },
                { 3, 9 },
                { 4, 12 }
            };
            try
            {
                var deptID = risksRecords[0].Department_ID;
                var currentOpenedSchedule = _contextRisk.RR_Schedule.FirstOrDefault(a => a.Status == "Open");
                var lastSubmission = _contextRisk.RR_Submission.FirstOrDefault(a => a.Submission_ID == currentOpenedSchedule.Submission_ID && a.Department_ID == deptID);

                if (currentOpenedSchedule.Submission_Quarter != 0)
                {
                    quarter = quarters[Convert.ToInt16(currentOpenedSchedule.Submission_Quarter)];
                }

                if (currentOpenedSchedule == null)
                {
                    return BadRequest("No opened submission");
                }
                if (lastSubmission != null)
                {
                    return BadRequest("Already submitted");
                }

                var submissions = new List<RR_Submission>();

                foreach (var risk in risksRecords)
                {
                    var submission = new RR_Submission
                    {
                        Submission_ID = currentOpenedSchedule.Submission_ID,
                        Risk_ID = risk.Risk_ID.Trim(),
                        Department_ID = risk.Department_ID,
                        Submission_Type = currentOpenedSchedule.Submission_Type.Trim(),
                        Submission_Month = currentOpenedSchedule.Submission_Type.Trim() == "Monthly" ? currentOpenedSchedule.Submission_Month : quarter,
                        Submission_Quarter = currentOpenedSchedule.Submission_Quarter ?? 0,
                        Submission_Year = currentOpenedSchedule.Submission_Year,
                        Impact = risk.Impact,
                        Likelihood = risk.Likelihood,
                        Inherent_level = risk.Inherent_level,
                        Status = risk.Status!.Trim(),
                        Control_Score = risk.Control_Score,
                        Residual_Score = risk.Residual_Score,
                        Risk_Level = risk.Risk_Level!.Trim(),
                        Risk_id_GT = risk.ID,
                        SubStatus = 0,
                        ApprovalStage = 0,
                        Modified = 0
                    };
                    submissions.Add(submission);
                }
                _contextRisk.RR_Submission.AddRange(submissions);
                await _contextRisk.SaveChangesAsync();

                string dept = _contextRisk.departments.Where(a => a.id == risksRecords[0].Department_ID).FirstOrDefault().department;

                var actionLog = new RR_Action_Log
                {
                    StaffID = user,
                    ActionDate = DateTime.Now,
                    Action = $"Submitted risk score for deparment, {dept}",
                };
                _contextRisk.RR_Action_Log.Add(actionLog);
                await _contextRisk.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error in Create method :::: {ex.Message}");
            }

            return Ok();
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
                ViewBag.DeptHead = HttpContext.Session.GetString("RRegisterDeptHead");
                ViewBag.LoginRole = HttpContext.Session.GetString("LoginRole");
                ViewBag.Message = "Welcome!";
                return true;
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error setting user session data on RiskEntryController :::: {ex.Message}");
            }
            return false;


        }
    }
}
