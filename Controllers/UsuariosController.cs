using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;
namespace RotinaXP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    [ProducesResponseType(typeof(List<Usuario>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Tarefas)
            .ToListAsync();
        return Ok(usuarios);
    }
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
public record CreateUsuarioRequest(string Nome, string Email, string SenhaHash, int Pontos = 0);
public record UpdateUsuarioRequest(string? Nome, string? Email, int? Pontos);
