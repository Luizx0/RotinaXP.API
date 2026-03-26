using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;


namespace RotinaXP.API.Controllers;

/// <summary>
/// Controller responsável pelas operações CRUD de usuários.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa uma nova instância do UsuariosController.
    /// </summary>
    /// <param name="context">O contexto de banco de dados.</param>
    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todos os usuários
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Usuario>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Tarefas)
            .ToListAsync();
        return Ok(usuarios);
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Tarefas)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
            return NotFound(new { mensagem = "Usuário não encontrado" });

        return Ok(usuario);
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Usuario), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { mensagem = "Nome e Email são obrigatórios" });

        var usuarioExistente = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuarioExistente != null)
            return BadRequest(new { mensagem = "Email já cadastrado" });

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = request.SenhaHash,
            Pontos = request.Pontos
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    /// <summary>
    /// Atualiza um usuário
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensagem = "Usuário não encontrado" });

        if (!string.IsNullOrWhiteSpace(request.Nome))
            usuario.Nome = request.Nome;

        if (!string.IsNullOrWhiteSpace(request.Email))
            usuario.Email = request.Email;

        if (request.Pontos.HasValue)
            usuario.Pontos = request.Pontos.Value;

        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deleta um usuário
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound(new { mensagem = "Usuário não encontrado" });

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

/// <summary>
/// Request para criação de um usuário.
/// </summary>
/// <param name="Nome">Nome completo do usuário.</param>
/// <param name="Email">Email do usuário.</param>
/// <param name="SenhaHash">Hash da senha do usuário.</param>
/// <param name="Pontos">Pontos iniciais do usuário (padrão: 0).</param>
public record CreateUsuarioRequest(string Nome, string Email, string SenhaHash, int Pontos = 0);

/// <summary>
/// Request para atualização de um usuário.
/// </summary>
/// <param name="Nome">Novo nome do usuário (opcional).</param>
/// <param name="Email">Novo email do usuário (opcional).</param>
/// <param name="Pontos">Novos pontos do usuário (opcional).</param>
public record UpdateUsuarioRequest(string? Nome, string? Email, int? Pontos);
