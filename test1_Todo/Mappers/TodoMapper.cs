using test1_Todo.Models;
using test1_Todo.ViewModels;

namespace test1_Todo.Mappers
{
    public class TodoMapper
    {
        public static Todo GetTodoFromAddTodoVM(todoAddVM vm) // la classe qui n as pas d'attribut c est mieux que soit static
        {
            //Todo todo = new Todo();
            //todo.libelle = vm.Libelle; 
            //todo.description = vm.description;
            //todo.DateLimite = vm.datelimite;
            //todo.Status = vm.Statut;
            //return todo; 
            return new Todo
            {
                libelle = vm.Libelle,
                description = vm.description,
                DateLimite = vm.datelimite,
                Status = vm.Statut
            };
        }
    }
}
