using Microsoft.AspNetCore.Mvc;
using ServiceUser;
using DominioUser;
using ServiceUser.DTOs;
namespace api_user.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Email e Senha são obrigatórios.");
            }

            try
            {
                var user = await _userService.AuthenticateAsync(dto.Email, dto.Password);

                if (user == null)
                {
                    // Evita dar dicas específicas (usuário não existe vs. senha incorreta)
                    return Unauthorized("Credenciais inválidas.");
                }

                // Em uma aplicação real, aqui você geraria um JWT (JSON Web Token)
                return Ok(user);
            }
            catch (Exception ex)
            {
                // Captura erros de infraestrutura do Service
                return StatusCode(500, $"Erro interno ao tentar fazer login: {ex.Message}");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("Email e Nova Senha são obrigatórios.");
            }

            try
            {
                // O Service deve: 1. Encontrar o usuário por email. 2. Hash a nova senha. 3. Salvar.
                var updated = await _userService.ResetPasswordAsync(dto.Email, dto.NewPassword);

                // Retorna 200 OK (ou 204 No Content se preferir) sem expor dados sensíveis
                return Ok(new { Message = "Sua senha foi redefinida com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                // Geralmente NotFound se o email não existir, ou Conflict se a senha for igual.
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao redefinir a senha: {ex.Message}");
            }
        }

        

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (result == null)
                return NotFound("Usuário não encontrado.");

            return Ok(result);
        }

        [HttpGet("api/{id}")]
        public async Task<IActionResult> GetByIdApi(int id)
        {
            var result = await _userService.GetUserByIdApiAsync(id);

            if (result == null)
                return NotFound("Usuário não encontrado.");

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create(UserCreateDto dto)
        {
            try
            {
                var created = await _userService.AddUserAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserUpdateDto dto)
        {
            try
            {
                var updated = await _userService.UpdateUserAsync(id, dto);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        
        

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _userService.DeleteUserAsync(id);
                return Ok(deleted);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

   
}

