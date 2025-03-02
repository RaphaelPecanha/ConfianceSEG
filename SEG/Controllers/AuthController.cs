using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SEG.Models;
using SEG.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SEG.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    // Injeção de dependências necessárias para as operações de autenticação e gerenciamento de usuários/roles.
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    // Construtor com injeção de dependências.
    public AuthController(ITokenService tokenService,
                          UserManager<ApplicationUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          IConfiguration configuration,
                          ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova role no sistema.
    /// </summary>
    /// <param name="roleName">Nome da role a ser criada.</param>
    /// <returns>Resposta HTTP com o status da operação.</returns>
    [HttpPost]
    [Route("CreateRole")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        // Verifica se a role já existe
        var roleExist = await _roleManager.RoleExistsAsync(roleName);

        if (!roleExist)
        {
            // Se a role não existir, cria uma nova role
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (roleResult.Succeeded)
            {
                _logger.LogInformation(1, "Roles Added");
                return StatusCode(StatusCodes.Status200OK, new Response
                {
                    Status = "Sucess",
                    Message = $"Role {roleName} added succesfully"
                });
            }
            else
            {
                _logger.LogInformation(2, "Error");
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    Status = "Error",
                    Message = $"Issue adding the new {roleName} role"
                });
            }
        }

        // Retorna erro se a role já existir
        return StatusCode(StatusCodes.Status400BadRequest, new Response
        {
            Status = "Error",
            Message = "Role already exist"
        });
    }

    /// <summary>
    /// Adiciona um usuário a uma role específica.
    /// </summary>
    /// <param name="email">Email do usuário.</param>
    /// <param name="roleName">Nome da role a ser atribuída.</param>
    /// <returns>Resposta HTTP com o status da operação.</returns>
    [HttpPost]
    [Route("AddUserToRole")]
    public async Task<IActionResult> AddUserToRole(string email, string roleName)
    {
        // Busca o usuário pelo email
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            // Tenta adicionar o usuário à role informada
            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                _logger.LogInformation(1, $"User {user.Email} added to the {roleName} role");
                return StatusCode(StatusCodes.Status200OK, new Response
                {
                    Status = "Sucess",
                    Message = $"User {user.Email} added to the {roleName} role"
                });
            }
            else
            {
                _logger.LogInformation(1, $"Error: Unable to add user {user.Email} to the {roleName} role");
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    Status = "Error",
                    Message = $"Error: Unable to add user {user.Email} to the {roleName} role"
                });
            }
        }

        // Retorna erro caso o usuário não seja encontrado
        return BadRequest(new { error = "Unable to find user" });
    }

    /// <summary>
    /// Realiza o login do usuário e gera os tokens de acesso e refresh.
    /// </summary>
    /// <param name="model">Dados de login contendo o nome de usuário e senha.</param>
    /// <returns>Objeto com o token de acesso, refresh token e data de expiração.</returns>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // Busca o usuário pelo nome de usuário
        var user = await _userManager.FindByNameAsync(model.UserName!);

        // Verifica se o usuário existe e se a senha está correta
        if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password!))
        {
            // Recupera as roles associadas ao usuário
            var userRoles = await _userManager.GetRolesAsync(user);

            // Cria as claims que serão inseridas no token
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Adiciona as roles como claims
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // Gera o token de acesso usando as claims definidas e a configuração
            var token = _tokenService.GenerateAcessToken(authClaims, _configuration);

            // Gera o refresh token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Recupera o tempo de validade do refresh token a partir da configuração
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

            // Define o tempo de expiração e armazena o refresh token no usuário
            user.RefreshTokenExpirtyTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);
            user.RefreshToken = refreshToken;

            // Atualiza o usuário com o refresh token e a nova data de expiração
            await _userManager.UpdateAsync(user);

            // Retorna o token de acesso, refresh token e data de expiração
            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = token.ValidTo
            });
        }

        // Retorna não autorizado se as credenciais estiverem incorretas
        return Unauthorized();
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// </summary>
    /// <param name="model">Dados de registro do usuário (nome, email e senha).</param>
    /// <returns>Resposta HTTP com o status da operação.</returns>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // Verifica se já existe um usuário com o mesmo nome
        var userExists = await _userManager.FindByNameAsync(model.UserName!);

        if (userExists != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Response
            {
                Status = "Error",
                Message = "Usuário já existe"
            });
        }

        // Cria um novo objeto ApplicationUser com os dados de registro
        ApplicationUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.UserName
        };

        // Tenta criar o usuário no sistema com a senha informada
        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Response
            {
                Status = "Error",
                Message = "Criação do usuário falhou."
            });
        }

        // Retorna sucesso caso o usuário seja criado
        return Ok(new Response
        {
            Status = "Sucess",
            Message = "Usuário criado com sucesso"
        });
    }

    /// <summary>
    /// Atualiza o token de acesso usando um refresh token.
    /// </summary>
    /// <param name="tokenModel">Objeto contendo o token de acesso expirado e o refresh token.</param>
    /// <returns>Novos tokens caso a operação seja bem-sucedida.</returns>
    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
    {
        if (tokenModel is null)
        {
            return BadRequest("Invalid client request");
        };

        string? acessToken = tokenModel.AcessToken ?? throw new ArgumentNullException(nameof(tokenModel));
        string? refreshToken = tokenModel.RefreshToken ?? throw new ArgumentNullException(nameof(tokenModel));

        // Verifica se o token de acesso ainda é válido para evitar renovações desnecessárias
        var handler = new JwtSecurityTokenHandler();
        ar jwtSecurityToken = handler.ReadJwtToken(acessToken);

        if (jwtSecurityToken.ValidTo > DateTime.UtcNow)
        {
            return BadRequest("O token de acesso ainda é válido.");
        }

        // Extrai o ClaimsPrincipal a partir do token de acesso expirado
        var principal = _tokenService.GetPrincipalFromExpiredToken(acessToken!, _configuration);
        if (principal == null)
        {
            return BadRequest("Invalid access token/refresh token");
        }

        string userName = principal.Identity.Name;
        var user = await _userManager.FindByNameAsync(userName!);

        // Correção da verificação de expiração do Refresh Token
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpirtyTime > DateTime.Now)
        {
            return BadRequest("Invalid access token/refresh token");
        }

        // Gera um novo token de acesso e refresh token seguro
        var newAcessToken = _tokenService.GenerateAcessToken(principal.Claims.ToList(), _configuration);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Adiciona um identificador único para garantir que apenas o último refresh token seja válido
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpirtyTime = DateTime.Now.AddMinutes(_configuration.GetValue<int>("JWT:RefreshTokenValidityInMinutes"));

        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(newAcessToken),
            RefreshToken = newRefreshToken,
            Expiration = newAcessToken.ValidTo
        });
    }

    /// <summary>
    /// Revoga o refresh token de um usuário, efetivamente invalidando o token de atualização.
    /// Requer autorização para acessar este endpoint.
    /// </summary>
    /// <param name="username">Nome de usuário do qual o refresh token será revogado.</param>
    /// <returns>Status 204 (No Content) em caso de sucesso ou erro caso o usuário não seja encontrado.</returns>
    [Authorize]
    [HttpPost]
    [Route("revoke/{username}")]
    public async Task<IActionResult> Revoke(string username)
    {
        // Busca o usuário pelo nome
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
        {
            return BadRequest("Usuário inválido");
        }

        // Revoga o refresh token atribuindo null
        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);

        // Retorna status 204 (No Content) para indicar que a operação foi concluída sem retorno de conteúdo
        return NoContent();
    }
}
