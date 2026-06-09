using AutoMapper;
using DominioDenuncia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceDenuncia;
using ServiceDenuncia.DTOs;


namespace ServiceDenuncia.Profiles
{
    public class DenunciaProfile : Profile
    {
        public DenunciaProfile()
        {
            // Endereço
            CreateMap<EnderecoDto, Endereco>().ReverseMap();

            // Create → Entity
            CreateMap<DenunciaCreateDto, Denuncia>()
                .ForMember(dest => dest.CriadoEm,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(_ => DenunciaStatus.Nova))
                .ForMember(dest => dest.TipoViolencia,
                    opt => opt.MapFrom(src =>
                        Enum.Parse<TipoViolencia>(src.TipoViolencia.ToString())));

            // Entity → DTO
            CreateMap<Denuncia, DenunciaDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TipoViolencia,
                    opt => opt.MapFrom(src => src.TipoViolencia.ToString())).ReverseMap();

            CreateMap<UserBasicDto, DenunciaCreateDto>().ForMember(d => d.NomeDenunciante,
                opt => opt.MapFrom(src => src.Name))
                .ForMember(d => d.Cpf,
                opt => opt.MapFrom(src => src.Cpf));


        }
    }
}
