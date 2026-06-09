using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser
{
    public class EnderecoDto
    {
        public int Id { get; set; }
        public string Rua { get; set; } = null!;
        public int Numero { get; set; }
        public string? Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
       
    }
}
