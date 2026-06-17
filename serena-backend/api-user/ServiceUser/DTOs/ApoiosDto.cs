using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser.DTOs
{
    public class ApoiosDto
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string Telefone { get; set; } = null!;

    }
}
