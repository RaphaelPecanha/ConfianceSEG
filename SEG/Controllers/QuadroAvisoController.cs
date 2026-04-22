using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SEG.Context;

namespace Confiance.SEG.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuadroAvisoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<QuadroAvisoController> _logger;

    public QuadroAvisoController(AppDbContext context, ILogger<QuadroAvisoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var avisos = _context.QuadroAvisos?.ToList();
        return Ok(avisos);
    }
}
