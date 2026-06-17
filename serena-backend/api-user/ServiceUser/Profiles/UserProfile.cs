using AutoMapper;
using DominioUser;
using ServiceUser.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser.Profiles
{
    public class UserProfile: Profile
    {
        public UserProfile()
        {


            // ----------- ENTIDADE → DTO (Leitura) -----------
            CreateMap<User, UserReadDTO>()
                .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => MaskCpf(src.Cpf)));
            CreateMap<User, UserInternalDTO>();

            CreateMap<Endereco, EnderecoDto>(); // Remova o mapeamento manual do Id para UserId
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // hash no service
                .ForMember(dest => dest.Endereco, opt => opt.MapFrom(src => src.Endereco))
                .ForMember(dest => dest.NumerosDeApoio, opt => opt.MapFrom(src => src.NumerosDeApoio));

            CreateMap<Apoios, ApoiosDto>();

            // ----------- DTO → ENTIDADE (Atualização) -----------

            // Configuração para evitar alteração de chaves primárias
            CreateMap<EnderecoDto, Endereco>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())    // NUNCA atualize o Id
                .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId é controlado pelo Service

            CreateMap<ApoiosDto, Apoios>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())    // Ignora Id para não tentar mudar a PK
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())    // Impede alteração do Id do usuário
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                // O mapeamento de listas (Endereco e Apoios) será manual no seu Service, 
                // então podemos pedir para o AutoMapper ignorar para evitar conflitos no merge manual
                .ForMember(dest => dest.Endereco, opt => opt.Ignore())
                .ForMember(dest => dest.NumerosDeApoio, opt => opt.Ignore());
        }

        private static string MaskCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length < 11)
                return cpf;
            // ex: 12345678901 -> ***.***.890-1 (exemplo simples)
            return $"***.***.{cpf.Substring(6, 3)}-{cpf.Substring(9)}";
        }
    }
}
