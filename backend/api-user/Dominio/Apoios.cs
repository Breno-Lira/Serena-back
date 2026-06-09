using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DominioUser
{
    public class Apoios
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK para User
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? Nome { get; set; }

        [Required, Phone, MaxLength(20)]
        public string Telefone { get; set; } = null!;

        // Navegação inversa 1:N
        public User? User { get; set; }
    }
}
