using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominioUser
{
    public class Endereco
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK para User
        public int UserId { get; set; }

        [Required, MaxLength(200)]
        public string Rua { get; set; } = null!;

        // Numero como string para permitir "s/n", "Apto 3", etc.
        [MaxLength(30)]
        public int Numero { get; set; }

        [MaxLength(100)]
        public string? Bairro { get; set; }

        [MaxLength(100)]
        public string? Cidade { get; set; }

        [MaxLength(100)]
        public string? Estado { get; set; }

        // Navegação inversa 1:1
        public User? User { get; set; }

    }



}
