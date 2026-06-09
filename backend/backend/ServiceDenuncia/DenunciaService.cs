using AutoMapper;
using DominioDenuncia;
using InfrastructureGeneric;
using Microsoft.EntityFrameworkCore;
using DominioDenuncia;

namespace ServiceDenuncia
{
    public class DenunciaService : IDenunciaService
    {
        private readonly IGenericRepository<Denuncia> _denunciaRepository;
        private readonly IMapper _mapper;
        private readonly IUserApiClient _userClient;

        public DenunciaService(
            IGenericRepository<Denuncia> denunciaRepo,
            IMapper mapper,
            IUserApiClient userClient)
        {
            _denunciaRepository = denunciaRepo;
            _mapper = mapper;
            _userClient = userClient;
        }

        public async Task<DenunciaDto> CreatDenunciaAsync(DenunciaCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            Console.WriteLine($"[LOG] Iniciando criação. UsuarioId: {dto.UsuarioId}");

            try
            {
                var resp = await _userClient.UserExistsAsync(dto.UsuarioId.Value);
                if (resp == null) throw new ApplicationException("Usuário não encontrado.");

                Console.WriteLine($"[LOG] Usuário encontrado. Tentando mesclar dados do Usuário no DTO...");

                // CUIDADO: Verifique se existe CreateMap<UserResponse, DenunciaCreateDto>() no seu Profile
                _mapper.Map(resp, dto);
            }
            catch (AutoMapperMappingException ex)
            {
                Console.WriteLine("--- ERRO AO MESCLAR USUÁRIO NO DTO ---");
                ImprimirDetalhesAutoMapper(ex);
                throw;
            }
            catch (Exception ex) { throw new ApplicationException("Erro ao verificar usuário.", ex); }

            try
            {
                if (!string.IsNullOrEmpty(dto.Cpf))
                {
                    // Remove tudo que não for número (inclusive aspas, pontos, traços e espaços)
                    dto.Cpf = new string(dto.Cpf.Where(char.IsDigit).ToArray());
                    Console.WriteLine($"[LOG] CPF formatado: {dto.Cpf}");
                }
                Console.WriteLine("[LOG] Tentando mapear DTO para Entidade Denuncia...");
                var entity = _mapper.Map<Denuncia>(dto);

                entity.CriadoEm = DateTime.UtcNow;
                entity.Status = DenunciaStatus.Nova;

                await _denunciaRepository.AddAsync(entity);
                await _denunciaRepository.SaveChangesAsync();

                return _mapper.Map<DenunciaDto>(entity);
            }
            catch (AutoMapperMappingException ex)
            {
                Console.WriteLine("--- ERRO AO MAPEAR DTO PARA ENTIDADE ---");
                ImprimirDetalhesAutoMapper(ex);
                throw;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"[ERRO BANCO]: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Método auxiliar para não repetir código de print
        private void ImprimirDetalhesAutoMapper(AutoMapperMappingException ex)
        {
            Console.WriteLine($"Mensagem: {ex.Message}");
            Console.WriteLine($"Tipo Origem: {ex.TypeMap?.SourceType.Name}");
            Console.WriteLine($"Tipo Destino: {ex.TypeMap?.DestinationType.Name}");
            if (ex.InnerException != null)
                Console.WriteLine($"Detalhe Interno: {ex.InnerException.Message}");
        }

        public async Task<IEnumerable<DenunciaDto>> GetAllDenunciasAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("UsuárioId inválido.", nameof(userId));

            try
            {
                var list = await _denunciaRepository
                    .FindAllByForeignKeyAsync("UsuarioId", userId);

                // Segurança: evita null propagado
                if (list == null || !list.Any())
                    return Enumerable.Empty<DenunciaDto>();

                return _mapper.Map<IEnumerable<DenunciaDto>>(list);
            }
            catch (TimeoutException ex)
            {
                // Banco demorou a responder
                throw new ApplicationException(
                    "Tempo limite excedido ao consultar denúncias.", ex);
            }
            catch (OperationCanceledException ex)
            {
                // Cancelamento explícito
                throw new ApplicationException(
                    "Operação cancelada ao buscar denúncias.", ex);
            }
            catch (Exception ex)
            {
                // Erro inesperado
                throw new ApplicationException(
                    "Erro inesperado ao buscar denúncias do usuário.", ex);
            }
        }


        public async Task<DenunciaDto?> GetDenunciaByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido.");

            var entity = await _denunciaRepository.GetByIdWithIncludesAsync(id,e=> e.Endereco);
            return entity == null ? null : _mapper.Map<DenunciaDto>(entity);
        }

        public async void UpdateByIdUser(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido.");
            try
            {


                var lista = GetAllDenunciasAsync(id);

                foreach (var dto in lista.Result)
                {
                    Denuncia denuncia = _mapper.Map<Denuncia>(dto);
                    denuncia.Status = DenunciaStatus.EmAnalise;
                    denuncia.UsuarioId = null;
                    _denunciaRepository.UpdateAsync(denuncia);

                }
                _denunciaRepository.SaveChangesAsync();
                
            }
            catch (DbUpdateException ex)
            {
                throw new ApplicationException("Erro ao atualizar denúncias no banco.", ex);
            }
        }
    }
}

