using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDenuncia
{
    public class DenunciaDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public int? UsuarioId { get; set; }
        public string? NomeDenunciante { get; set; }
        public string? Cpf { get; set; }

        public EnderecoDto? Endereco { get; set; }
        public string Status { get; set; }
        public string TipoViolencia { get; set; }
        public DateTime CriadoEm { get; set; }

    }
}
