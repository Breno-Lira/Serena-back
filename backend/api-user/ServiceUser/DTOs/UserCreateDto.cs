using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser
{
    public class UserCreateDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }   // recebido em claro; trate com hash no service
        public string Cpf { get; set; }

        public string Rg { get; set; }
        public string Telefone { get; set; }
        public DateTime DataNascimento { get; set; }
        public EnderecoDto Endereco { get; set; } = new EnderecoDto();
        public List<ApoiosDto>? NumerosDeApoio { get; set; } = new List<ApoiosDto>();
    }
}
