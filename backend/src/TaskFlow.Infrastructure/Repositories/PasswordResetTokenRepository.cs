using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// パスワードリセットトークンリポジトリの EF Core 実装
/// </summary>
public class PasswordResetTokenRepository : PasswordResetTokenRepositoryInterface
{
    private readonly AppDbContext _context;

    /// <summary>
    /// PasswordResetTokenRepository を生成する
    /// </summary>
    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// トークンハッシュでパスワードリセットトークンを検索する
    /// </summary>
    public async Task<PasswordResetToken?> FindByTokenHashAsync(string tokenHash)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    /// <summary>
    /// パスワードリセットトークンを作成する
    /// </summary>
    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    /// <summary>
    /// パスワードリセットトークンを更新する
    /// </summary>
    public async Task UpdateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }
}
