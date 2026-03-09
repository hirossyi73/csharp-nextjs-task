using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// ユーザーリポジトリの EF Core 実装
/// </summary>
public class UserRepository : UserRepositoryInterface
{
    private readonly AppDbContext _context;

    /// <summary>
    /// UserRepository を生成する
    /// </summary>
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// メールアドレスでユーザーを検索する
    /// </summary>
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// ID でユーザーを検索する
    /// </summary>
    public async Task<User?> FindByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    /// <summary>
    /// ユーザーを作成する
    /// </summary>
    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// ユーザー情報を更新する
    /// </summary>
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
