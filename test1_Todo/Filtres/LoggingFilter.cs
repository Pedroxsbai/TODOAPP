using Microsoft.AspNetCore.Mvc.Filters;
using test1_Todo.Services;

namespace test1_Todo.Filtres
{
    public class LoggingFilter : ActionFilterAttribute
    {
        private readonly ILoggingService _loggingService;  // la dependance est injecte une seul fois 

        public LoggingFilter(ILoggingService loggingService) 
        {
            _loggingService = loggingService; // initialisation de la dependance (FAIBLE COUPLAGE , BON PRATIQUE) 
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unkown"; // unkown pour eviter l exception si jamais la valeur est nulle
            var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
            var userName = context.HttpContext.Session.GetString("UserName") ?? "Anonymous"; // si l utilisateur n est pas connecte , on met anonymous

            _loggingService.LogAction(userName, controllerName, actionName);
        }
    }
}
