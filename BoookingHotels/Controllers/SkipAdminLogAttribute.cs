using Microsoft.AspNetCore.Mvc.Filters;

namespace BoookingHotels.Controllers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipAdminLogAttribute : Attribute
    {
    }
}