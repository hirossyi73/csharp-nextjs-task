using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// ユーザーリポジトリのインタフェース
/// </summary>
public interface UserRepositoryInterface
{
    /// <summary>
    /// メールアドレスでユーザーを検索する
    /// </summary>
    Task<User?> FindByEmailAsync(string email);

    /// <summary>
    /// ID でユーザーを検索する
    /// </summary>
    Task<User?> FindByIdAsync(Guid id);

    /// <summary>
    /// ユーザーを作成する
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// ユーザー情報を更新する
    /// </summary>
    Task UpdateAsync(User user);
}
