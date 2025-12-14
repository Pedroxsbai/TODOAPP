using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using test1_Todo.Services;

namespace test1_Todo.Filtres
{
    public class ThemeFilter : ActionFilterAttribute
    {
        private readonly IThemeService _themeService;

        public ThemeFilter(IThemeService themeService)
        {
            _themeService = themeService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
                        var theme = _themeService.GetCurrentTheme(context.HttpContext);
            
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.Theme = theme;
            }
        }
    }
}
