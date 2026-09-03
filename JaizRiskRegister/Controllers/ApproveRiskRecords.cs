using JaizRiskRegister.Models;
using Microsoft.AspNetCore.Mvc;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class ApproveRiskRecords : Controller
    {
        private readonly RiskDbContext _contextRisk;
        Helpers.Utility util = new Helpers.Utility();

        public ApproveRiskRecords(RiskDbContext contextRisk)
        {
            _contextRisk = contextRisk;
        }

        // THIS GETS A LIST OF ALL DEPARTMENTS WHOSE RISK SCORES HAVE BEEN APPROVED BY RMDUSER AND AWAITING FINAL APPROVAL 
        public IActionResult ScoresApproval()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            var riskScores = (from JaizRiskSub in _contextRisk.RR_Submission
                              join departments in _contextRisk.departments
                              on JaizRiskSub.Department_ID equals departments.id
                              where JaizRiskSub.ApprovalStage == 1
                              group new
                              {
                                  JaizRiskSub.Submission_ID,
                                  JaizRiskSub.Risk_ID,
                                  JaizRiskSub.Submission_Year,
                                  JaizRiskSub.Submission_Type
                              }
                              by new { departments.department, departments.id } into g
                              select new
                              {
                                  Department = g.Key.department,
                                  RiskDepartmentId = g.Key.id,
                                  SubmissionId = g.Min(x => x.Submission_ID),
                                  RiskId = g.Min(x => x.Risk_ID),
                                  SubmissionYear = g.Max(x => x.Submission_Year),
                                  RiskSubmissionType = g.First().Submission_Type
                              }).ToList();

            return Json(riskScores);
        }

        // THIS GETS A LIST OF ALL DEPARTMENTS THAT THEIR RISK RECORDS THAT HAVE BEEN EDITED AND AWAITING APPROVAL
        public IActionResult RecordApproval()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            var risksRecords = (from JaizRisks in _contextRisk.RR_General_Template
                                join departments in _contextRisk.departments
                                on JaizRisks.Department_ID equals departments.id
                                where JaizRisks.Modified == 1
                                select departments.department).Distinct().ToList();

            return Json(risksRecords);
        }

        //THIS GETS A LIST OF RISK RECORDS ACCORDING TO DEPARTMENT
        public IActionResult GetRecordsForApproval(int dept)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            if (dept != null)
            {
                var departmentalRisks = (from JaizRisks in _contextRisk.RR_General_Template
                                         where JaizRisks.Department_ID == dept
                                         select new
                                         {
                                             JaizRisks.Department_ID,
                                             JaizRisks.Risk_ID,
                                             JaizRisks.Type_of_Risk,
                                             JaizRisks.Risk_Area,
                                             JaizRisks.Risk_Description,
                                             JaizRisks.Business_Unit_Impacted,
                                             JaizRisks.Management_Controls,
                                             JaizRisks.Action_Plan,
                                             JaizRisks.Assigned_To,
                                             JaizRisks.Modified,
                                             JaizRisks.IsApproved,

                                         }).ToList();

                return Json(new { success = true, data = departmentalRisks });
            }

            return Json(new { success = false, message = "No records found." });
        }
        [HttpGet]

        public IActionResult GetScoresForApproval(int riskDepartmentId, int submissionId, string riskSubmissionType)
        {
            try
            {
                if (string.IsNullOrEmpty(riskSubmissionType))
                {
                    return Json(new { success = false, message = "Invalid submission type." });
                }

                if (riskDepartmentId != 0 && submissionId != 0 && !string.IsNullOrEmpty(riskSubmissionType))
                {
                    var riskType = riskSubmissionType.Trim().ToLower();

                    if (riskType == "monthly" || riskType == "quarterly")
                    {
                        var departmentalRisks = (from JaizRisks in _contextRisk.RR_General_Template
                                                 join DepartmentalRiskSubmission in _contextRisk.RR_Submission
                                                 on JaizRisks.Risk_ID equals DepartmentalRiskSubmission.Risk_ID
                                                 where DepartmentalRiskSubmission.Department_ID == riskDepartmentId &&
                                                       DepartmentalRiskSubmission.Submission_ID == submissionId &&
                                                       DepartmentalRiskSubmission.Submission_Type == riskSubmissionType &&
                                                       DepartmentalRiskSubmission.ApprovalStage == 1
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
                                                     DepartmentalRiskSubmission.Modified
                                                 }).ToList();

                        if (departmentalRisks.Count == 0)
                        {
                            return Json(new { success = false, message = "No records found." });
                        }

                        return Json(new { success = true, data = departmentalRisks });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

            return Json(new { success = false, message = "Invalid request." });
        }
        public IActionResult GetDepartment(string dept)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            if (!string.IsNullOrEmpty(dept))
            {
                var deptRecord = _contextRisk.departments.FirstOrDefault(a => a.department == dept);
                if (deptRecord != null)
                {
                    HttpContext.Session.SetInt32("DepartmentID", deptRecord.id);
                }
            }

            return View("Records");
        }
        public IActionResult GetScores(int riskDepartmentId, int submissionId, string riskSubmissionType)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            if (!string.IsNullOrEmpty(riskSubmissionType))
            {
                HttpContext.Session.SetInt32("RiskDepartmentId", riskDepartmentId);
                HttpContext.Session.SetInt32("SubmissionId", submissionId);
                HttpContext.Session.SetString("RiskSubmissionType", riskSubmissionType);
            }

            return View("Scores");
        }
        [HttpPost]
        public IActionResult ApproveReords([FromBody] List<RiskUpdateModel> updatedRisks)
        {
            var user = HttpContext.Session.GetString("user");

            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {

                foreach (var riskUpdate in updatedRisks)
                {
                    var existingRisk = _contextRisk.RR_General_Template.FirstOrDefault(r => r.Department_ID == riskUpdate.Department_ID && r.Risk_ID == riskUpdate.Risk_ID);

                    //if (existingRisk.EditedBy == user)
                    //{
                    //    return BadRequest("Cannot approve");
                    //}

                    if (existingRisk != null)
                    {
                        existingRisk.Modified = 0;
                        existingRisk.IsApproved = 1;

                        _contextRisk.Update(existingRisk);
                    }
                }
                _contextRisk.SaveChanges();

                string dept = _contextRisk.departments.Where(a => a.id == updatedRisks[0].Department_ID).FirstOrDefault().department;

                var actionLog = new RR_Action_Log
                {
                    StaffID = user,
                    ActionDate = DateTime.Now,
                    Action = $"Approved Edited Risk Records for Department, {dept}.",
                };
                _contextRisk.RR_Action_Log.Add(actionLog);
                _contextRisk.SaveChanges();

                return Ok();

            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error on ApproveRecords method :::: {ex.Message}");
            }
            return BadRequest("Failed to approve");
        }

        public IActionResult ApproveScores(int department_ID, int submission_ID, string submission_Type)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                var user = HttpContext.Session.GetString("user");

                var editedScores = _contextRisk.RR_Submission
                    .Where(r => r.Department_ID == department_ID &&
                                r.Submission_ID == submission_ID &&
                                r.Submission_Type == submission_Type)
                    .ToList();

                if (!editedScores.Any())
                {
                    return NotFound("No records found to approve.");
                }

                foreach (var scoreUpdate in editedScores)
                {
                    if (scoreUpdate.EditedBy == user)
                    {
                        return BadRequest("Cannot approve");
                    }

                    scoreUpdate.SubStatus = 1;
                    scoreUpdate.ApprovalStage = 2;
                    scoreUpdate.Modified = 0;
                    _contextRisk.Update(scoreUpdate);  // Updating each item individually
                }
                _contextRisk.SaveChanges();

                string dept = _contextRisk.departments.Where(a => a.id == department_ID).FirstOrDefault().department;

                var actionLog = new RR_Action_Log
                {
                    StaffID = user,
                    ActionDate = DateTime.Now,
                    Action = $"Approved Edited Risk Scores for Department {dept}.",
                };

                _contextRisk.RR_Action_Log.Add(actionLog);
                _contextRisk.SaveChanges();

                var deptId = Convert.ToInt32(department_ID);
                var dept2 = _contextRisk.departments.Where(a => a.id == deptId);

                if (dept != null)
                {
                    foreach (var department in dept2)
                    {
                        util.SendNotificationEmail_2($"{department.head}@jaizbankplc.com", $"Head of {department.department}");
                    }
                }
                return Ok("Risk approved successfully!");
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error on ApproveScores method :::: {ex.Message}");
            }

            return BadRequest("Failed to approve");
        }

        public class RiskUpdateModel
        {
            public int Department_ID { get; set; }
            public string Risk_ID { get; set; }
            public int Modified { get; set; }
            public int IsApproved { get; set; }
        }

        public class ScoreUpdateModel
        {
            public int Department_ID { get; set; }
            public int Submission_ID { get; set; }
            public string Submission_Type { get; set; }
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
                util.WriteToLog($"Error setting user session data :::: {ex.Message}");
            }
            return false;
        }
    }
}

