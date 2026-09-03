using JaizRiskRegister.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class EditRiskRecordController : Controller
    {
        private readonly RiskDbContext _contextRisk;
        Helpers.Utility util = new Helpers.Utility();
        public EditRiskRecordController(RiskDbContext contextRisk)
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
        public IActionResult GetDepartmentalRisks(int departmentId)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                if (departmentId != null)
                {
                    var departmentalRisks = (from JaizRisks in _contextRisk.RR_General_Template
                                             where JaizRisks.Department_ID == departmentId
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

                    return Json(departmentalRisks);
                }
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error on GetDepartmentalRisks method :::: {ex.Message}");
            }
            return Json(new { success = false, message = "Unable to fetch records." });
        }

        [HttpPost]
        public IActionResult UpdateRiskRecord([FromBody] List<RiskUpdateModel> updatedRisks)
        {
            var user = HttpContext.Session.GetString("user");

            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                // Check for duplicate Risk_ID in the incoming data
                var duplicateRiskIds = updatedRisks
                    .GroupBy(r => r.Risk_ID)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateRiskIds.Any())
                {
                    return BadRequest("Duplicate Risk_IDs");
                }

                foreach (var riskUpdate in updatedRisks)
                {
                    var existingRisk = _contextRisk.RR_General_Template
                        .FirstOrDefault(r => r.Department_ID == riskUpdate.Department_ID && r.Risk_ID == riskUpdate.Risk_ID);

                    if (existingRisk != null)
                    {
                        existingRisk.Type_of_Risk = riskUpdate.RiskType;
                        existingRisk.Risk_Area = riskUpdate.RiskArea;
                        existingRisk.Risk_Description = riskUpdate.RiskDescription;
                        existingRisk.Business_Unit_Impacted = riskUpdate.BusinessUnitImpacted;
                        existingRisk.Management_Controls = riskUpdate.ManagementKeyControls;
                        existingRisk.Action_Plan = riskUpdate.ActionPlan;
                        existingRisk.Assigned_To = riskUpdate.AssignedTo;
                        existingRisk.Modified = riskUpdate.Modified;
                        existingRisk.IsApproved = riskUpdate.IsApproved;
                        existingRisk.EditedBy = user;

                        _contextRisk.Update(existingRisk);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(riskUpdate.Risk_ID))
                        {
                            return BadRequest("Risk ID");
                        }
                        // Insert new record
                        var newRisk = new RR_General_Template
                        {
                            Department_ID = riskUpdate.Department_ID,
                            Risk_ID = riskUpdate.Risk_ID,
                            Type_of_Risk = riskUpdate.RiskType,
                            Risk_Area = riskUpdate.RiskArea,
                            Risk_Description = riskUpdate.RiskDescription,
                            Business_Unit_Impacted = riskUpdate.BusinessUnitImpacted,
                            Management_Controls = riskUpdate.ManagementKeyControls,
                            Action_Plan = riskUpdate.ActionPlan,
                            Assigned_To = riskUpdate.AssignedTo,
                            Modified = riskUpdate.Modified,
                            IsApproved = riskUpdate.IsApproved,
                            EditedBy = user,
                        };
                        _contextRisk.RR_General_Template.Add(newRisk);
                    }
                }
                _contextRisk.SaveChanges();

                string dept = _contextRisk.departments.Where(a => a.id == updatedRisks[0].Department_ID).FirstOrDefault().department;

                var actionLog = new RR_Action_Log
                {
                    StaffID = user,
                    ActionDate = DateTime.Now,
                    Action = $"Edited Risk Record for Department, {dept}.",
                };
                _contextRisk.RR_Action_Log.Add(actionLog);
                _contextRisk.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error in UpdateRiskRecord method :::: {ex.Message}");
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
                ViewBag.LoginRole = HttpContext.Session.GetString("LoginRole");

                return true;
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error setting user session data on EditRecord Controller method :::: {ex.Message}");
            }

            return false;
        }
        public class RiskUpdateModel
        {
            public int Department_ID { get; set; }
            public string Risk_ID { get; set; }
            public string RiskType { get; set; }
            public string RiskArea { get; set; }
            public string RiskDescription { get; set; }
            public string BusinessUnitImpacted { get; set; }
            public string ManagementKeyControls { get; set; }
            public string ActionPlan { get; set; }
            public string AssignedTo { get; set; }
            public int Modified { get; set; }
            public int IsApproved { get; set; }
        }
    }
}
