using JaizRiskRegister.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly RiskDbContext _contextRisk;
        Helpers.Utility util = new Helpers.Utility();
        public AuditLogController(RiskDbContext contextRisk)
        {
            _contextRisk = contextRisk;
        }
        public ActionResult Index(int? page)
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                int pageSize = 10; // Number of records per page
                int pageNumber = (page ?? 1); // Default to page 1 if no page number is provided

                IPagedList<RR_Action_Log> records = _contextRisk.RR_Action_Log
                    .OrderByDescending(a => a.ActionDate)
                    .ToPagedList(pageNumber, pageSize);

                return View(records);
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error getting audit logs :::: {ex.Message}");
            }

            return BadRequest("Failed to get details");
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
                ViewBag.RMDApprover = HttpContext.Session.GetString("RRegisterApprover");
                    
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
