using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominioDenuncia
{
    public class Endereco
    {
        public int Id { get; set; }
        public int DenunciaId { get; set; }
        public string Rua { get; set; }
        public int Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }

        public Denuncia Denuncia { get; set; }

    }
}
