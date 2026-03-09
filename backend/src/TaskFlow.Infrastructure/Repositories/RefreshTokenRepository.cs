using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// リフレッシュトークンリポジトリの EF Core 実装
/// </summary>
public class RefreshTokenRepository : RefreshTokenRepositoryInterface
{
    private readonly AppDbContext _context;

    /// <summary>
    /// RefreshTokenRepository を生成する
    /// </summary>
    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// トークンハッシュでリフレッシュトークンを検索する
    /// </summary>
    public async Task<RefreshToken?> FindByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    /// <summary>
    /// リフレッシュトークンを作成する
    /// </summary>
    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    /// <summary>
    /// リフレッシュトークンを更新する
    /// </summary>
    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 指定ユーザーの全リフレッシュトークンを無効化する
    /// </summary>
    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true));
    }
}
