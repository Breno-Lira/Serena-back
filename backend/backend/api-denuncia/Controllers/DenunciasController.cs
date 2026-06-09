using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceDenuncia;             // IDenunciaService

namespace api_denuncia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DenunciasController : ControllerBase
    {
        private readonly IDenunciaService _denunciaService;

        public DenunciasController(IDenunciaService denunciaService)
        {
            _denunciaService = denunciaService ?? throw new ArgumentNullException(nameof(denunciaService));
        }

        /// <summary>
        /// Cria uma nova denúncia.
        /// POST /api/denuncias
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DenunciaDto>> Create(DenunciaCreateDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Payload não pode ser vazio." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _denunciaService.CreatDenunciaAsync(dto);

                // Retorna 201 com o recurso criado; pressupõe que result.Id existe.
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (ApplicationException appEx)
            {
                // Erros controlados da camada de serviço (DB, mapeamento, etc.)
                return Problem(detail: appEx.Message, statusCode: 500, title: "Erro ao processar a solicitação");
            }
            catch (Exception ex)
            {
                // Erro inesperado
                return Problem(detail: ex.Message, statusCode: 500, title: "Erro interno");
            }
        }

        [HttpGet("all/{userId:int}")]
        public async Task<ActionResult<IEnumerable<DenunciaDto>>> GetAll(int userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Query string 'userId' é obrigatória e deve ser maior que zero." });

            try
            {
                var list = await _denunciaService.GetAllDenunciasAsync(userId);
                return Ok(list);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (ApplicationException appEx)
            {
                return Problem(detail: appEx.Message, statusCode: 500, title: "Erro ao recuperar denúncias");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500, title: "Erro interno");
            }
        }

        /// <summary>
        /// Recupera uma denúncia por id.
        /// GET /api/denuncias/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DenunciaDto>> GetById([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Id inválido." });

            try
            {
                var entity = await _denunciaService.GetDenunciaByIdAsync(id);
                if (entity == null)
                    return NotFound(new { message = $"Denúncia com id {id} não encontrada." });

                return Ok(entity);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (ApplicationException appEx)
            {
                return Problem(detail: appEx.Message, statusCode: 500, title: "Erro ao recuperar denúncia");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500, title: "Erro interno");
            }
        }
        [HttpDelete("DeleteUser/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {


                _denunciaService.UpdateByIdUser(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500, title: "Erro interno");
            }
        }
    }
}
