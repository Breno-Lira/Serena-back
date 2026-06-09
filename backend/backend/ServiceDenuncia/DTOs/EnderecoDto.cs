using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDenuncia
{
    public class EnderecoDto
    {
        [Required]
        public string Rua { get; set; }
        public int Numero { get; set; }
        public string Bairro { get; set; }
        [Required]
        public string Cidade { get; set; }
        [Required]
        public string Estado { get; set; }
        
    }
}
