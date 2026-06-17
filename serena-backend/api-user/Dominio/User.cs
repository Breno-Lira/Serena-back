using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DominioUser
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }    

        [Required]
        public string Cpf { get; set; }

        [Required]
        public string Rg { get; set; }
        [Required]
        public string Telefone { get; set; }

        [Required]
        public DateTime? DataNascimento { get; set; }
        [Required]
        public Endereco Endereco { get; set; } 
        public ICollection<Apoios>? NumerosDeApoio { get; set; } // Opcional
    }
}
