using AutoMapper;
using DominioUser;
using Microsoft.EntityFrameworkCore; // Importante para excecoes do DB
using InfrastructureUser.Repositories;
using ServiceUser.DTOs;

namespace ServiceUser
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public UserService(IGenericRepository<User> userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserReadDTO> AddUserAsync(UserCreateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                // 1. Validacao de regra de negocio (lanca InvalidOperationException em caso de duplicidade)
                await VerifyUniqueFieldsAsync(dto.Email, dto.Cpf, dto.Rg);

                // 2. Mapeamento
                var user = _mapper.Map<User>(dto);
                user.PasswordHash = HashPassword(dto.Password);

                // 3. Persistencia
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                return _mapper.Map<UserReadDTO>(user);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    $"Falha na persistencia do usuario. Erro no banco de dados: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new InvalidOperationException(
                    $"Erro ao mapear UserCreateDto para a entidade User. Detalhes: {ex.Message}", ex);
            }
        }

        public async Task<UserReadDTO> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // 1. Carrega a entidade existente com as relacoes
            var existing = await _userRepository.GetByIdWithIncludesAsync(id, u => u.Endereco, u => u.NumerosDeApoio);
            if (existing == null)
                throw new InvalidOperationException("Usuario nao encontrado.");

            await VerifyUniqueFieldsAsync(dto.Email, null, null, id);

            // 2. Mapeia campos simples (Nome, Email, etc.)
            _mapper.Map(dto, existing);

            // 1:1 Endereco (merge seguro)
            if (dto.Endereco != null)
            {
                if (existing.Endereco == null)
                {
                    var novoEndereco = _mapper.Map<Endereco>(dto.Endereco);
                    novoEndereco.UserId = existing.Id;
                    existing.Endereco = novoEndereco;
                }
                else
                {
                    _mapper.Map(dto.Endereco, existing.Endereco);
                    existing.Endereco.UserId = existing.Id;
                }
            }

            // 1:N Apoios (sincronizacao da colecao)
            var incomingApoios = dto.NumerosDeApoio ?? new List<ApoiosDto>();
            existing.NumerosDeApoio ??= new List<Apoios>();
            var incomingIds = incomingApoios.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

            // Remover os que nao vieram no DTO
            var toRemove = existing.NumerosDeApoio.Where(a => !incomingIds.Contains(a.Id)).ToList();
            foreach (var rem in toRemove)
                existing.NumerosDeApoio.Remove(rem);

            // Adicionar novos ou atualizar existentes
            foreach (var apoioDto in incomingApoios)
            {
                if (apoioDto.Id == 0)
                {
                    var novo = _mapper.Map<Apoios>(apoioDto);
                    novo.UserId = existing.Id;
                    existing.NumerosDeApoio.Add(novo);
                }
                else
                {
                    var atual = existing.NumerosDeApoio.FirstOrDefault(a => a.Id == apoioDto.Id);
                    if (atual != null)
                        _mapper.Map(apoioDto, atual);
                }
            }

            // 3. Persistencia
            await _userRepository.UpdateAsync(existing);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserReadDTO>(existing);
        }

        public async Task<UserReadDTO> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException($"Usuario com ID {id} nao encontrado para exclusao.");

            var userReadDto = _mapper.Map<UserReadDTO>(user);

            try
            {
                await _userRepository.DeleteAsync(id);
                await _userRepository.SaveChangesAsync();
                return userReadDto;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
            {
                throw new InvalidOperationException(
                    $"Nao foi possivel excluir o usuario ID {id} pois ele possui registros associados.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    $"Falha ao excluir o usuario ID {id}. Detalhes: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        public async Task<UserReadDTO?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdWithIncludesAsync(
                id, u => u.Endereco, u => u.NumerosDeApoio);

            return user == null ? null : _mapper.Map<UserReadDTO>(user);
        }

        public async Task<UserInternalDTO?> GetUserByIdApiAsync(int id)
        {
            var user = await _userRepository.GetByIdWithIncludesAsync(
                id, u => u.Endereco, u => u.NumerosDeApoio);

            return user == null ? null : _mapper.Map<UserInternalDTO>(user);
        }

        public async Task<UserReadDTO?> AuthenticateAsync(string email, string password)
        {
            var users = await _userRepository.GetByConditionAsync(u => u.Email == email);
            var user = users.FirstOrDefault();

            // Nao diferencia "usuario inexistente" de "senha incorreta" (boa pratica de seguranca)
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return _mapper.Map<UserReadDTO>(user);
        }

        public async Task<UserReadDTO> ResetPasswordAsync(string email, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(novaSenha))
                throw new ArgumentException("Email e nova senha sao obrigatorios para redefinicao.");

            var users = await _userRepository.GetByConditionAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            if (user == null)
                throw new InvalidOperationException($"Usuario com e-mail '{email}' nao encontrado.");

            var novoHash = HashPassword(novaSenha);
            if (novoHash == user.PasswordHash)
                throw new InvalidOperationException("A nova senha nao pode ser igual a senha atual.");

            user.PasswordHash = novoHash;

            try
            {
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                return _mapper.Map<UserReadDTO>(user);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    $"Falha ao redefinir a senha do usuario {email}. Erro no banco: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        // ------------------------------------------------------------------
        // Helpers privados
        // ------------------------------------------------------------------

        // Verifica unicidade de email/cpf/rg. Lanca InvalidOperationException quando ja existe.
        // cpf/rg sao opcionais (no update so validamos o email).
        private async Task VerifyUniqueFieldsAsync(string email, string? cpf, string? rg, int? ignoreId = null)
        {
            var existingEmail = await _userRepository.GetByConditionAsync(u =>
                u.Email == email && (ignoreId == null || u.Id != ignoreId));
            if (existingEmail.Any())
                throw new InvalidOperationException("Ja existe um usuario com este email.");

            if (!string.IsNullOrWhiteSpace(cpf))
            {
                var existingCpf = await _userRepository.GetByConditionAsync(u =>
                    u.Cpf == cpf && (ignoreId == null || u.Id != ignoreId));
                if (existingCpf.Any())
                    throw new InvalidOperationException("Ja existe um usuario com este CPF.");
            }

            if (!string.IsNullOrWhiteSpace(rg))
            {
                var existingRg = await _userRepository.GetByConditionAsync(u =>
                    u.Rg == rg && (ignoreId == null || u.Id != ignoreId));
                if (existingRg.Any())
                    throw new InvalidOperationException("Ja existe um usuario com este RG.");
            }
        }

        // ATENCAO: hashing apenas ilustrativo. Em producao use BCrypt/PBKDF2/Argon2.
        private static string HashPassword(string password)
            => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

        private static bool VerifyPassword(string providedPassword, string storedHash)
            => HashPassword(providedPassword) == storedHash;
    }
}
