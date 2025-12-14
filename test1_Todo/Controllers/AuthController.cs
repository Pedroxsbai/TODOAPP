using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using test1_Todo.Mappers;
using test1_Todo.ViewModels;
using test1_Todo.Models;
using System.Text.Json;
using test1_Todo.Services;
using test1_Todo.Filtres;

namespace test1_Todo.Controllers
{
    [ServiceFilter(typeof(ThemeFilter))] //
    [ServiceFilter(typeof(LoggingFilter))] // 
    public class AuthController : Controller
    {
        ISessionManagerService session;
        public AuthController(ISessionManagerService session)
        {
            this.session = session;
        }
        public IActionResult Inscription()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Inscription(AuthVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Auth auth = AuthMapper.GetAuthFromAuthVM(vm);
            session.SetString("UserName", auth.Nom, HttpContext);
            session.SetString("IsConnected", "True", HttpContext);
            
            return RedirectToAction("Index" , "Todo");
        }

    }
}
