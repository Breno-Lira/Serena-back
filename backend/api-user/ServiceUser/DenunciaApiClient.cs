using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser
{
    public class DenunciaApiClient: IDenunciaApiClient
    {
        private readonly HttpClient _http;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        public DenunciaApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task DeletDenunciaByUserIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("UsuarioId inválido para deleção de denúncias.");
                }

                var resp = await _http.DeleteAsync($"/api/denuncias/DeleteUser/{userId}"); // Use DeleteAsync se for um DELETE na API

                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    // Log do erro real para você debugar
                    Console.WriteLine($"Erro da API externa: {error}");
                    throw new ApplicationException($"Erro ao deletar as denuncias: {error}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Log ex.Message aqui antes de dar o throw
                throw new ApplicationException("Erro de conexão ao serviço de denúncias.", ex);
            }
            catch (Exception ex)
            {
                // Aqui o throw agora será capturado corretamente pelo sistema
                throw new ApplicationException("Erro ao processar deleção das denúncias.", ex);
            }
        }
    }
}
