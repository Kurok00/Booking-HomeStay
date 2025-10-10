using BoookingHotels.Data;
using BoookingHotels.Models;
using BoookingHotels.Controllers;
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
        // Kiểm tra xem action có skip logging không
        var skipLogging = context.ActionDescriptor.EndpointMetadata
            .Any(m => m is SkipAdminLogAttribute);
        
        if (skipLogging)
            return;

        var httpContext = context.HttpContext;
        var user = httpContext.User;

        if (user.Identity != null && user.Identity.IsAuthenticated && user.IsInRole("Admin"))
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim?.Value == null)
                return;
                
            if (!int.TryParse(userIdClaim.Value, out int adminId))
                return;

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
