using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser.DTOs
{
    public class UserInternalDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Cpf { get; set; }
        public string Rg { get; set; }
        public string Telefone { get; set; }
        public DateTime DataNascimento { get; set; }

        public List<ApoiosDto> NumerosDeApoio { get; set; } = new List<ApoiosDto>();
        public EnderecoDto Endereco { get; set; }
    }
}
