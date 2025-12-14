using test1_Todo.Enum;

namespace test1_Todo.Models
{
    public class Todo
    {
        public String libelle { get; set; }
        public String description { get; set; }
        public DateTime DateLimite { get; set; }
        public State Status { get; set; }
    }

}
