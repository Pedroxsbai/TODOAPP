using test1_Todo.Models;
using test1_Todo.ViewModels;

namespace test1_Todo.Mappers
{
    public class AuthMapper
    {
        public static Auth GetAuthFromAuthVM(AuthVM VM)
        {
            return new Auth 

            { Nom = VM.Nom,
                Email = VM.Email,
                Password = VM.Password, 
            };
        } 
        

}
}
