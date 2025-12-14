using System.ComponentModel.DataAnnotations;

namespace test1_Todo.ViewModels
{
    public class AuthVM
    {
        [Required]
        public String Nom { get; set; }
        [Required]
        [EmailAddress]
        public String Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }
    }
}
