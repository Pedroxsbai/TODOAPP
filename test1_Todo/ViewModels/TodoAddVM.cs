using System;
using System.ComponentModel.DataAnnotations;
using test1_Todo.Enum;

namespace test1_Todo.ViewModels
{
    public class todoAddVM
    {
        [Required(ErrorMessage="vou deviez remplissez le libelle")]
        public string? Libelle { get; set; }
        [Required(ErrorMessage=("La description est obligatoire!"))]
        public string? description { get; set; }
        [DataType(DataType.Date)]
        public DateTime datelimite { get; set; }
        [Required]
        public State Statut  { get; set; }
    }

}
