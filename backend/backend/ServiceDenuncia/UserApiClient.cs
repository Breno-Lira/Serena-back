using Microsoft.Extensions.Caching.Memory;
using ServiceDenuncia.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDenuncia
{
    public class UserApiClient: IUserApiClient
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        public UserApiClient(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public async Task<UserBasicDto> UserExistsAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("UsuarioId inválido para verificação.");
            }

            var cacheKey = $"user_basic_{userId}";

            if (_cache.TryGetValue(cacheKey, out UserBasicDto cached))
                return cached;

            try
            {
                // Assume endpoint GET /api/users/{id}/exists returns 200 (exists) or 404 (not exists)
                var resp = await _http.GetAsync($"/api/User/api/{userId}");

                

                if(resp.StatusCode == HttpStatusCode.NotFound)
                    throw new ApplicationException("Usuário não encontrado.");
                

                
                var user = await resp.Content.ReadFromJsonAsync<UserBasicDto>();

                if (user == null)
                    throw new ApplicationException("Resposta inválida do serviço de usuários.");
                _cache.Set(cacheKey, user, CacheTtl);

                return user;
            }
            catch (OperationCanceledException) // timeout or cancellation
            {
                throw new TimeoutException("Timeout ao validar usuário no serviço de usuários.");
            }
            catch (HttpRequestException)
            {
                // serviço indisponível ou erro de rede
                throw new ApplicationException("Erro ao chamar serviço de usuários.");
            }
        }
    }
}

