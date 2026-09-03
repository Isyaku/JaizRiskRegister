using JaizRiskRegister.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class SubmissionSchedule : Controller
    {
        private readonly RiskDbContext _contextRisk;
        Helpers.Utility util = new Helpers.Utility();
        public SubmissionSchedule(RiskDbContext contextRisk)
        {
            _contextRisk = contextRisk;
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RR_Schedule model)
        {
            var user = HttpContext.Session.GetString("user");

            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            if (model.Submission_Type == "Monthly" && (!model.Submission_Month.HasValue || model.Submission_Year == 0))
            {
                ModelState.AddModelError(string.Empty, "Please select a valid month and year for monthly submission.");
                return View(model);
            }

            if (model.Submission_Type == "Quarterly" && (!model.Submission_Quarter.HasValue || model.Submission_Year == 0))
            {
                ModelState.AddModelError(string.Empty, "Please select a valid quarter and year for quarterly submission.");
                return View(model);
            }

            var currentSchedule = _contextRisk.RR_Schedule.FirstOrDefault(a => a.Status == "Open");

            if (currentSchedule != null)
            {
                ViewBag.ErrMessage = "You have an opened submission window that is yet to be closed!";
                return View();
            }

            if (ModelState.IsValid)
            {
                if (model.Submission_Type == "Monthly")
                {
                    model.Submission_Quarter = 0;
                }
                else if (model.Submission_Type == "Quarterly")
                {
                    model.Submission_Month = 0;
                }

                var existingSchedule = _contextRisk.RR_Schedule.FirstOrDefault(a => a.Submission_Type.Trim() == model.Submission_Type && a.Submission_Month == model.Submission_Month && a.Submission_Quarter == model.Submission_Quarter && a.Submission_Year == model.Submission_Year);

                if (existingSchedule != null)
                {
                    ViewBag.ErrMessage = "This schedule already exist.";
                    return View();
                }

                var submission = new RR_Schedule
                {
                    Submission_Type = model.Submission_Type,
                    Submission_Month = model.Submission_Month,
                    Submission_Quarter = model.Submission_Quarter,
                    Submission_Year = model.Submission_Year,
                    Status = "Open",
                    Created_By = user
                };
                _contextRisk.RR_Schedule.Add(submission);
                await _contextRisk.SaveChangesAsync();

                var actionLog = new RR_Action_Log
                {
                    StaffID = user,
                    ActionDate = DateTime.Now,
                    Action = $"Created submission schedule for MONTH: {model.Submission_Month}, QUARTER: {model.Submission_Quarter}, YEAR: {model.Submission_Year}",
                };
                _contextRisk.RR_Action_Log.Add(actionLog);
                await _contextRisk.SaveChangesAsync();

                var schedules = _contextRisk.RR_Schedule.OrderByDescending(a => a.Submission_ID).ToList();

                var dept = _contextRisk.departments.Where(a => a.head != "");

                if (dept != null)
                {
                    foreach (var department in dept)
                    {
                        util.SendNotificationEmail($"{department.head}@jaizbankplc.com", $"Head of {department.department}", $"{model.Submission_Year}", $"{model.Submission_Type}");
                    }
                }
                ViewBag.Message = "Schedule created successfully!";

                return View("Get", schedules);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            var schedules = _contextRisk.RR_Schedule.OrderByDescending(a => a.Submission_ID).OrderByDescending(a => a.Submission_ID).ToList();
            return View(schedules);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int Id)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            var user = HttpContext.Session.GetString("user");
            var submission = _contextRisk.RR_Schedule.FirstOrDefault(s => s.Submission_ID == Id);

            if (submission == null)
            {
                return NotFound();
            }

            var submissionType = submission.Submission_Quarter > 0 && submission.Submission_Quarter <= 4
                ? $"{submission.Submission_Quarter} quarter of" : $"{submission.Submission_Month} month of";

            submission.Status = "Closed";

            var actionLog = new RR_Action_Log
            {
                StaffID = user,
                ActionDate = DateTime.Now,
                Action = $"Closed submission for {submissionType} {submission.Submission_Year}",
            };
            _contextRisk.RR_Action_Log.Add(actionLog);
            await _contextRisk.SaveChangesAsync();

            var schedules = _contextRisk.RR_Schedule.OrderByDescending(a => a.Submission_ID).ToList();
            return View("Get", schedules);
        }

        public IActionResult ProfileSubmitter()
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
        public JsonResult GetDepartmentHead(int id)
        {
            var dept = _contextRisk.departments.FirstOrDefault(d => d.id == id);
            if (dept == null)
            {
                return Json(new { success = false, message = "Department not found." });
            }

            return Json(new { success = true, head = dept.head });
        }

        [HttpPost]
        public JsonResult UpdateDepartmentHead(int id, string newHead)
        {
            var user = HttpContext.Session.GetString("user");
            var dept = _contextRisk.departments.FirstOrDefault(d => d.id == id);
            if (dept == null)
            {
                return Json(new { success = false, message = "Department not found." });
            }

            dept.head = newHead.Trim();

            var actionLog = new RR_Action_Log
            {
                StaffID = user,
                ActionDate = DateTime.Now,
                Action = $"Updated head for {dept.department} to {newHead}.",
            };
            _contextRisk.RR_Action_Log.Add(actionLog);
            _contextRisk.SaveChanges();

            return Json(new { success = true, message = "Department head updated successfully." });
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
                ViewBag.LoginRole = HttpContext.Session.GetString("LoginRole");

                return true;
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error setting user session data in SubmissionSchedule controller :::: {ex.Message}");
            }
            return false;
        }
    }
}
