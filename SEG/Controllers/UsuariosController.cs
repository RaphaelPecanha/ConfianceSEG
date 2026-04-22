using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using SEG.Context;
using Confiance.SEG.CrossCutting.Filters;
using Confiance.SEG.Domain;
using Confiance.SEG.Application.DTOs;
using Confiance.SEG.Infrastructure.Repositories;
using Confiance.SEG.Application.Interfaces;
using SEG.Models;
using Microsoft.Extensions.Logging;

namespace Confiance.SEG.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuariosController> _logger;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public UsuariosController(AppDbContext context, ILogger<UsuariosController> logger, IUnitOfWork uow,
                                  ITokenService tokenService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _uow = uow;
            _tokenService = tokenService;
            _userManager = userManager;
            _configuration = configuration;
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
        public async Task<ActionResult<UsuarioLoginResponseDTO>> PostValidarUsuario([FromBody] UsuarioLoginDTO usuarioLogin)
        {
            if (usuarioLogin is null)
                return BadRequest("Dados inválidos");

            _logger.LogInformation($" ======= Acessando PostValidarUsuario: {usuarioLogin.Login}");

            // Criptografa a senha para MD5 antes da consulta na tabela de usuários legada
            string senhaCriptografada = MD5HashGenerator.GenerateMD5Hash(usuarioLogin.Senha);

            var usuarioEncontrado = await _uow.UsuariosRepository
                .GetAsync(u => u.Login == usuarioLogin.Login && u.Senha == senhaCriptografada);

            if (usuarioEncontrado is null)
                return NotFound("Usuário não encontrado ou senha incorreta");

            // Sincroniza / obtém o usuário do Identity para gerar tokens
            var appUser = await _userManager.FindByNameAsync(usuarioLogin.Login);

            if (appUser == null)
            {
                // Tenta criar o usuário no Identity usando o email e a senha fornecida
                appUser = new ApplicationUser
                {
                    UserName = usuarioEncontrado.Login,
                    Email = usuarioEncontrado.Email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var createResult = await _userManager.CreateAsync(appUser, usuarioLogin.Senha);
                if (!createResult.Succeeded)
                {
                    _logger.LogWarning("Falha ao criar usuário Identity para {Login}", usuarioLogin.Login);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao sincronizar usuário de identidade");
                }
            }

            // Verifica a senha no Identity
            if (!await _userManager.CheckPasswordAsync(appUser, usuarioLogin.Senha))
            {
                return Unauthorized();
            }

            // Gera claims e tokens (similar ao AuthController.Login)
            var userRoles = await _userManager.GetRolesAsync(appUser);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, appUser.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, appUser.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = _tokenService.GenerateAcessToken(authClaims, _configuration);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

            appUser.RefreshTokenExpirtyTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);
            appUser.RefreshToken = refreshToken;
            await _userManager.UpdateAsync(appUser);

            // Monta o response combinando os dados do usuário legados e os tokens
            var response = new UsuarioLoginResponseDTO
            {
                Id = usuarioEncontrado.Id ?? 0,
                Login = usuarioEncontrado.Login,
                Nome = usuarioEncontrado.Nome,
                SobreNome = usuarioEncontrado.SobreNome,
                NomeCompleto = string.Join(" ", new[] { usuarioEncontrado.PrimeiroNome, usuarioEncontrado.SobreNome }.Where(s => !string.IsNullOrEmpty(s))),
                Setor = usuarioEncontrado.IdSetor ?? 0,
                Status = usuarioEncontrado.Status ? 1 : 0,
                Email = usuarioEncontrado.Email,
                CodSapiens = usuarioEncontrado.IdSapiens?.ToString(),
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = token.ValidTo,
                Menus = new List<List<object>>()
            };

            // Preencher menus a partir da tabela perm_usuarios_menu usando o Id do usuário
            try
            {
                var idUsuario = usuarioEncontrado.Id ?? 0;
                var permissoes = await _uow.Context.PermUsuariosMenu
                                    .Where(p => p.IdUsuario == idUsuario)
                                    .AsNoTracking()
                                    .ToListAsync();

                foreach (var p in permissoes)
                {
                    // Cada item interno: [path_menu, nivel_permissao]
                    response.Menus.Add(new List<object> { p.PathMenu ?? string.Empty, p.NivelPermissao });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar permissões de menu para o usuário {User}", usuarioEncontrado.Login);
            }

            return Ok(response);
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

            // IMPORTANTE: Se você armazena a senha criptografada em MD5 no banco de dados,
            // você deve criptografar a senha também neste método 'Post' de criação de usuário.
            string senhaCriptografada = MD5HashGenerator.GenerateMD5Hash(usuarioDto.Senha);

            var usuario = new Usuario
            {
                Id = usuarioDto.Id,
                Nome = usuarioDto.Nome,
                Login = usuarioDto.Login,
                // Armazena a senha criptografada
                Senha = senhaCriptografada
            };

            _uow.UsuariosRepository.Create(usuario);
            await _uow.CommitAsync();

            return new CreatedAtRouteResult("UsuarioAdd", new { id = usuario.Id }, usuario);
        }
    }
}