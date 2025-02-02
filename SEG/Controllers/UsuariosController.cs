using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEG.Context;
using SEG.Filters;
using SEG.Models;
using SEG.Repositories;

namespace SEG.Controllers
{

    // Com o action pra acessar os end points pelo nome do método
    [Route("[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(AppDbContext context, ILogger<UsuariosController> logger, IUnitOfWork uow)
        {
            _uow = uow;
            _logger = logger;
        }

        // Algumas restrições que podem ser colocado no verbo http para evitar consultas nos bancos desnecessárias
        // [HttpGet("id:int") -- Apenas numero inteiro
        // [HttpGet("id:alpha") -- Apenas alpha numeros
        // [HttpGet("id:bool") -- Apenas booleanos 0 / 1
        // [HttpGet("id:datetime") -- Apenas valores de DateTime
        // [HttpGet("id:decimal") -- Apenas numero decimais
        // Outros a mais que pode ser acrescentados
        // :length(5) - Tamanho de 5
        // :maxlength(10) - até 10
        // : minlength(5) - minimo 5

        [HttpGet] //Usuarios/Get
        [Authorize]
        public async Task<ActionResult<IEnumerable<Usuario>>> Get()
        {
            var usuarios = await _uow.UsuariosRepository.GetAllAsync();

            if (usuarios is null)
                return NotFound("Usuários não encontrado");

            return Ok(usuarios);
        }
        [HttpGet("/GetValidarUsuario/{login}/{senha}")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        /*public async Task<ActionResult<Usuario>> GetValidarUsuario([FromQuery]string login, string senha)*/
        public async Task<ActionResult<Usuario>> GetValidarUsuario(string login, string senha)
        {
            _logger.LogInformation($" ======= Acessando GetValidarUsuario: {login}");

            var usuario = await _uow.UsuariosRepository.GetAsync(u => u.Login == login && u.Senha == senha);

            if (usuario is null)
                return NotFound();

            return Ok(usuario);
           // return StatusCode(StatusCodes.Status200OK, "Sucesso");

        }
        [HttpGet("/GetById/{id:int:min(1)}")] //GetById/<id>
        [HttpGet("{id:int:min(1)}", Name = "UsuarioAdd")] //Usuarios/Get/<id>
        public async Task<ActionResult<Usuario>> Get(int id)
        {
            var usuario = await _uow.UsuariosRepository.GetAsync(u => u.Id == id);
            if (usuario is null)
                return NotFound();

            return usuario;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Usuario usuario)
        {
            if (usuario is null)
            {
                return BadRequest();
            }

            _uow.UsuariosRepository.Create(usuario);
            await _uow.CommitAsync();

            return new CreatedAtRouteResult("UsuarioAdd", new { id = usuario.Id }, usuario);
        }
    }
}
