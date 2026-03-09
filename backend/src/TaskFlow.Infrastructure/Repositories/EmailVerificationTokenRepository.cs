using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// メール確認トークンリポジトリの EF Core 実装
/// </summary>
public class EmailVerificationTokenRepository : EmailVerificationTokenRepositoryInterface
{
    private readonly AppDbContext _context;

    /// <summary>
    /// EmailVerificationTokenRepository を生成する
    /// </summary>
    public EmailVerificationTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// トークンハッシュでメール確認トークンを検索する
    /// </summary>
    public async Task<EmailVerificationToken?> FindByTokenHashAsync(string tokenHash)
    {
        return await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    /// <summary>
    /// メール確認トークンを作成する
    /// </summary>
    public async Task<EmailVerificationToken> CreateAsync(EmailVerificationToken token)
    {
        _context.EmailVerificationTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    /// <summary>
    /// メール確認トークンを更新する
    /// </summary>
    public async Task UpdateAsync(EmailVerificationToken token)
    {
        _context.EmailVerificationTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 指定ユーザーの未使用トークンを全て無効化する
    /// </summary>
    public async Task InvalidateByUserIdAsync(Guid userId)
    {
        var tokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsUsed = true;
        }

        await _context.SaveChangesAsync();
    }
}
