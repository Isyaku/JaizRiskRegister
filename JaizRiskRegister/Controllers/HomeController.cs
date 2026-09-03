using JaizRiskRegister.Helpers;
using JaizRiskRegister.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using static JaizRiskRegister.DbData.AppDbContext;

namespace JaizRiskRegister.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        Helpers.Utility util = new Helpers.Utility();
        //private readonly DeptOnPortalDbContext _contextDept;
        private readonly RiskDbContext _contextRisk;
        public HomeController(ILogger<HomeController> logger, /*DeptOnPortalDbContext contextDept*/ RiskDbContext contextRisk)
        {
            _logger = logger;
            //_contextDept = contextDept;
            _contextRisk = contextRisk;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userName = util.DecryptTextWithPrivateKey(model.UserName);
                var userPassword = util.DecryptTextWithPrivateKey(model.Password);

                //TEST CREDENTIALS FOR DIIFERENT USERS  RMDApprover!!!
                if (userPassword.Contains("RMDA!"))
                {
                    //RMD APPROVER
                    HttpContext.Session.SetString("user", userName);
                    HttpContext.Session.SetString("LoginRole", "Approver");
                    HttpContext.Session.SetString("RRegisterApprover", "RMDApprover");

                    return RedirectToAction("Welcome", "Home");
                }
                else if (userPassword.Contains("RMDU!"))
                {
                    //RMD USER
                    HttpContext.Session.SetString("user", userName);
                    HttpContext.Session.SetString("LoginRole", "Reviewer");
                    HttpContext.Session.SetString("RRegisterRMDUser", "RMDUser");

                    return RedirectToAction("Welcome", "Home");
                }
                else if (userPassword.Contains("DH!"))
                {
                    //DEPARTMENT HEAD
                    HttpContext.Session.SetString("user", userName);
                    HttpContext.Session.SetString("LoginRole", "RiskEntry");
                    HttpContext.Session.SetString("RRegisterDeptHead", "DeptHead");

                    return RedirectToAction("Welcome", "Home");
                }

                var isValidationSuccessful = ValidateUser(userName, userPassword);

                var isRMDUser = HttpContext.Session.GetString("RRegisterRMDUser");
                var isRMDApprover = HttpContext.Session.GetString("RRegisterApprover");

                var isDeptHead = _contextRisk.departments.Where(h => h.head == userName).FirstOrDefault();

                if (!isValidationSuccessful)
                {
                    ModelState.AddModelError("InvalidUsernameOrPassword", "The user name or password provided is incorrect.");
                }
                else if (isDeptHead == null && isRMDUser == null && isRMDApprover == null)
                {
                    ModelState.AddModelError("Unauthorized", "You don't have access to Jaiz Risk Register");
                }
                else if (isValidationSuccessful && (isDeptHead != null || isRMDUser != null || isRMDApprover != null))
                {
                    HttpContext.Session.SetString("user", userName);
                    return RedirectToAction("Welcome", "Home");
                }

                return View(model);

            }
            return View();
        }
        public IActionResult Welcome()
        {
            if (!SetSessionData())
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("user");
            return RedirectToAction("Login", "Home");
        }       
        private bool ValidateUser(string username, string password)
        {
            var userValidation = new JaizAuthService.JaizRoleManagerServiceClient(0);
            var logModel = new JaizAuthService.LogonModel()
            {
                username = username,
                password = password,
                appID = 72,
            };

            var result = new JaizAuthService.LoginResult();

            try
            {
                result = userValidation.ValidateADUser2FA(logModel);

                if (result.loggedIn)
                {
                    var role = result.roles[0];

                    if ( role == "RRegisterRMDUser")
                    {
                        HttpContext.Session.SetString("RRegisterRMDUser", "RMDUser");
                        HttpContext.Session.SetString("LoginRole", "Reviewer");

                        return true;
                    }

                    else if (role == "RRegisterApprover")
                    {
                        HttpContext.Session.SetString("RRegisterApprover", "RMDApprover");
                        HttpContext.Session.SetString("LoginRole", "Approver");

                        return true;
                    }

                    else if (role == "User")
                    {
                        HttpContext.Session.SetString("RRegisterDeptHead", "DeptHead");
                        HttpContext.Session.SetString("LoginRole", "RiskEntry");                        

                        return true;
                    }
                    
                    //else if (role == "RRegisterDeptHead")
                    //{
                    //    HttpContext.Session.SetString("RRegisterDeptHead", "DeptHead");
                    //    HttpContext.Session.SetString("LoginRole", "RiskEntry");

                    //    return true;
                    //}
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Aunthenticating User", ex.Message);
            }
            return result.loggedIn;
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
                ViewBag.DeptHead = HttpContext.Session.GetString("RRegisterDeptHead");
                ViewBag.Message = "Welcome!";
                return true;
            }
            catch (Exception ex)
            {
                util.WriteToLog($"Error setting user session data on Home Controller :::: {ex.Message}");
            }
            return false;
        }
    }
}