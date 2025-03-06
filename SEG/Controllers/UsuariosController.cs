using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEG.Context;
using SEG.Filters;
using SEG.Models;
using SEG.Models.DTO;
using SEG.Repositories;

namespace SEG.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(AppDbContext context, ILogger<UsuariosController> logger, IUnitOfWork uow)
        {
            _uow = uow;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Usuario>>> Get()
        {
            var usuarios = await _uow.UsuariosRepository.GetAllAsync();

            if (usuarios is null)
                return NotFound("Usuários não encontrados");

            return Ok(usuarios);
        }

        [HttpPost("ValidarUsuario")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<Usuario>> PostValidarUsuario([FromBody] UsuarioLoginDTO usuarioLogin)
        {
            if (usuarioLogin is null)
                return BadRequest("Dados inválidos");

            _logger.LogInformation($" ======= Acessando PostValidarUsuario: {usuarioLogin.Login}");

            var usuarioEncontrado = await _uow.UsuariosRepository
                .GetAsync(u => u.Login == usuarioLogin.Login && u.Senha == usuarioLogin.Senha);

            if (usuarioEncontrado is null)
                return NotFound("Usuário não encontrado");

            return Ok(usuarioEncontrado);
        }

        [HttpGet("{id:int:min(1)}", Name = "UsuarioAdd")]
        public async Task<ActionResult<Usuario>> Get(int id)
        {
            var usuario = await _uow.UsuariosRepository.GetAsync(u => u.Id == id);
            if (usuario is null)
                return NotFound();

            return usuario;
        }

        [HttpPost]
        //[Authorize(Policy = "Admin")] // Define que o usuário precisa da permissão de Admin pra acessar
        public async Task<ActionResult> Post([FromBody] UsuarioDTO usuarioDto)
        {
            if (usuarioDto is null)
            {
                return BadRequest();
            }

            var usuario = new Usuario
            {
                Id = usuarioDto.Id,
                Nome = usuarioDto.Nome,
                Login = usuarioDto.Login,
                Senha = usuarioDto.Senha
            };

            _uow.UsuariosRepository.Create(usuario);
            await _uow.CommitAsync();

            return new CreatedAtRouteResult("UsuarioAdd", new { id = usuario.Id }, usuario);
        }
    }
}
