namespace DominioDenuncia
{
    public class Denuncia
    {
        public int Id { get; set; }

        
        public string Descricao { get; set; }


        public int? UsuarioId { get; set; }

        public string? NomeDenunciante { get; set; }
        public string? Cpf { get; set; }

        public Endereco? Endereco { get; set; }


        // Status da denúncia
        public DenunciaStatus Status { get; set; } = DenunciaStatus.Nova;
        public TipoViolencia TipoViolencia { get; set; }

        // Datas
        public DateTime CriadoEm { get; set; }
       

        


    }
}
