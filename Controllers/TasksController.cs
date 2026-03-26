using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.Models;
namespace RotinaXP.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TarefasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public TarefasController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    [ProducesResponseType(typeof(List<Tarefa>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tarefas = await _context.Tarefas
            .Include(t => t.Usuario)
            .ToListAsync();
        return Ok(tarefas);
    }
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Tarefa), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var tarefa = await _context.Tarefas
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tarefa == null)
            return NotFound(new { mensagem = "Tarefa não encontrada" });
        return Ok(tarefa);
    }
    [HttpGet("usuario/{usuarioId}")]
    [ProducesResponseType(typeof(List<Tarefa>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUsuario(int usuarioId)
    {
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == usuarioId);
        if (!usuarioExiste)
            return NotFound(new { mensagem = "Usuário não encontrado" });
        var tarefas = await _context.Tarefas
            .Include(t => t.Usuario)
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();
        return Ok(tarefas);
    }
    [HttpPost]
    [ProducesResponseType(typeof(Tarefa), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTarefaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest(new { mensagem = "Título é obrigatório" });
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == request.UsuarioId);
        if (!usuarioExiste)
            return BadRequest(new { mensagem = "UsuarioId não encontrado" });
        var tarefa = new Tarefa
        {
            Titulo = request.Titulo,
            Concluida = request.Concluida,
            UsuarioId = request.UsuarioId
        };
        _context.Tarefas.Add(tarefa);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tarefa.Id }, tarefa);
    }
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTarefaRequest request)
    {
        var tarefa = await _context.Tarefas.FindAsync(id);
        if (tarefa == null)
            return NotFound(new { mensagem = "Tarefa não encontrada" });
        if (!string.IsNullOrWhiteSpace(request.Titulo))
            tarefa.Titulo = request.Titulo;
        if (request.Concluida.HasValue)
            tarefa.Concluida = request.Concluida.Value;
        _context.Tarefas.Update(tarefa);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var tarefa = await _context.Tarefas.FindAsync(id);
        if (tarefa == null)
            return NotFound(new { mensagem = "Tarefa não encontrada" });
        _context.Tarefas.Remove(tarefa);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
public record CreateTarefaRequest(string Titulo, bool Concluida, int UsuarioId);
public record UpdateTarefaRequest(string? Titulo, bool? Concluida);
