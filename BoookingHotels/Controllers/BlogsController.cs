using Microsoft.AspNetCore.Mvc;

namespace BoookingHotels.Controllers
{
    public class BlogsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            return View();
        }
    }
}
