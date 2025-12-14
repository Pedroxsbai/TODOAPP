using Microsoft.AspNetCore.Mvc;
using test1_Todo.Services;

namespace test1_Todo.Controllers
{

    public class ThemeController : Controller
    {
        private readonly IThemeService _themeService;

        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }
        public IActionResult Toggle()
        {
            _themeService.ToggleTheme(HttpContext);
            string referer = Request.Headers["Referer"].ToString(); // la page d ou vient l utilisateur 
            return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer); // redirection a referer sinon page d acceuil 
        }
    }
}
