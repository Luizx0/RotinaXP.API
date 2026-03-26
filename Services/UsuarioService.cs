using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Data;
using RotinaXP.API.DTOs;
using RotinaXP.API.Models;
using System.Security.Cryptography;
using System.Text;
namespace RotinaXP.API.Services;
public class UsuarioService
{
    private readonly ApplicationDbContext _context;
    public UsuarioService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<(bool Sucesso, UserDTO? Usuario, string Mensagem)> RegistrarAsync(RegisterRequest request)
    {
        // Validar se email já existe
        var usuarioExistente = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuarioExistente != null)
            return (false, null, "Email já cadastrado no sistema");
        // Validar dados de entrada
        if (string.IsNullOrWhiteSpace(request.Nome))
            return (false, null, "Nome é obrigatório");
        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            return (false, null, "Senha deve ter no mínimo 6 caracteres");
        try
        {
            // Criar novo usuário
            var usuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = HashPassword(request.Senha),
                Pontos = 0
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            var userDTO = MapToDTO(usuario);
            return (true, userDTO, "Usuário registrado com sucesso");
        }
        catch (Exception ex)
        {
            return (false, null, $"Erro ao registrar usuário: {ex.Message}");
        }
    }
    public async Task<(bool Sucesso, UserDTO? Usuario, string Mensagem)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return (false, null, "Email e senha são obrigatórios");
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario == null)
            return (false, null, "Usuário não encontrado");
        // Validar senha
        if (!VerifyPassword(request.Senha, usuario.SenhaHash))
            return (false, null, "Senha incorreta");
        var userDTO = MapToDTO(usuario);
        return (true, userDTO, "Login realizado com sucesso");
    }
    public async Task<UserDTO?> GetUsuarioByIdAsync(int id)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);
        return usuario == null ? null : MapToDTO(usuario);
    }
    public async Task<UserDTO?> GetUsuarioByEmailAsync(string email)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
        return usuario == null ? null : MapToDTO(usuario);
    }
    public async Task<(bool Sucesso, UserDTO? Usuario, string Mensagem)> AtualizarAsync(int id, UpdateUserRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return (false, null, "Usuário não encontrado");
        // Atualizar nome se fornecido
        if (!string.IsNullOrWhiteSpace(request.Nome))
            usuario.Nome = request.Nome;
        // Atualizar email se fornecido (validar duplicata)
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != usuario.Email)
        {
            var emailExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (emailExistente != null)
                return (false, null, "Email já cadastrado no sistema");
            usuario.Email = request.Email;
        }
        try
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            var userDTO = MapToDTO(usuario);
            return (true, userDTO, "Usuário atualizado com sucesso");
        }
        catch (Exception ex)
        {
            return (false, null, $"Erro ao atualizar usuário: {ex.Message}");
        }
    }
    public async Task<(bool Sucesso, string Mensagem)> DeletarAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return (false, "Usuário não encontrado");
        try
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return (true, "Usuário deletado com sucesso");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao deletar usuário: {ex.Message}");
        }
    }
    public async Task<(bool Sucesso, string Mensagem)> AdicionarPontosAsync(int id, int pontos)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return (false, "Usuário não encontrado");
        if (pontos < 0)
            return (false, "Quantidade de pontos não pode ser negativa");
        try
        {
            usuario.Pontos += pontos;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return (true, $"Adicionados {pontos} pontos ao usuário");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao adicionar pontos: {ex.Message}");
        }
    }
    public async Task<(bool Sucesso, string Mensagem)> DeduziPontosAsync(int id, int pontos)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return (false, "Usuário não encontrado");
        if (pontos < 0)
            return (false, "Quantidade de pontos não pode ser negativa");
        if (usuario.Pontos < pontos)
            return (false, "Saldo de pontos insuficiente");
        try
        {
            usuario.Pontos -= pontos;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return (true, $"Deduzidos {pontos} pontos do usuário");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao deduzir pontos: {ex.Message}");
        }
    }
    private UserDTO MapToDTO(Usuario usuario)
    {
        return new UserDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Pontos = usuario.Pontos
        };
    }
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }
}
