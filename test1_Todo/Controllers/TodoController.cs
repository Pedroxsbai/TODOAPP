using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using test1_Todo.Filtres;
using test1_Todo.Mappers;
using test1_Todo.Models;
using test1_Todo.Services;
using test1_Todo.ViewModels;

namespace test1_Todo.Controllers
{
    [ServiceFilter(typeof(ThemeFilter))] // utilisation pour l injection depandance
    [AuthFilres]
    public class TodoController : Controller
    {
        ISessionManagerService session;
        public TodoController(ISessionManagerService session)
        {
            this.session = session;
        }
        public object Valeur { get; private set; }

        [AuthFilres]
        public IActionResult Index()
        {
            List<Todo> todos = session.GetObject<List<Todo>>("todos", HttpContext) ?? new List<Todo>();
            return View(todos);
        }
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Add(todoAddVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            
            List<Todo> list = session.GetObject<List<Todo>>("todos", HttpContext) ?? new List<Todo>();
            
            Todo todo = TodoMapper.GetTodoFromAddTodoVM(vm);
            list.Add(todo);
            
            session.AddObject("todos", list, HttpContext);

            return RedirectToAction(nameof(Index));
        }
    }
}
