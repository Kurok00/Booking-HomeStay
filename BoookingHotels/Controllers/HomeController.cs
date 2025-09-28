using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Auth");
            }
        }

        return RedirectToAction("Index", "Auth");
    }

    [Authorize(Roles = "User")]
    public IActionResult UserDashboard()
    {
        return View();
    }
}
