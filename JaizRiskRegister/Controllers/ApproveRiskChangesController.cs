using JaizRiskRegister.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class ApproveRiskChangesController : Controller
    {
        Helpers.Utility util = new Helpers.Utility();

        public IActionResult Index()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
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
