using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class AdminActionLogAttribute : ActionFilterAttribute
{
    private readonly ApplicationDbContext _context;

    public AdminActionLogAttribute(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        if (user.Identity != null && user.Identity.IsAuthenticated && user.IsInRole("Admin"))
        {
            var adminId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);

            var action = context.ActionDescriptor.DisplayName;
            var controller = context.ActionDescriptor.RouteValues["controller"];
            var actionName = context.ActionDescriptor.RouteValues["action"];

            var log = new AdminLog
            {
                AdminId = adminId,
                Action = actionName ?? "Unknown",
                Entity = controller ?? "Unknown",
                Description = $"Admin thực hiện action {actionName} trên {controller}",
                CreatedAt = DateTime.Now
            };

            _context.AdminLogs.Add(log);
            _context.SaveChanges();
        }

        base.OnActionExecuted(context);
    }
}
