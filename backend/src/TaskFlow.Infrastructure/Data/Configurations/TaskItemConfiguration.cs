using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

/// <summary>
/// TaskItem エンティティの EF Core 設定。tasks テーブルへのマッピングとインデックスを定義する
/// </summary>
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    /// <summary>
    /// TaskItem エンティティのテーブル・カラム・インデックス・リレーションを設定する
    /// </summary>
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .HasDefaultValue(Domain.Enums.TaskStatus.NotStarted)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // ユーザーごとのタスク一覧取得用インデックス
        builder.HasIndex(t => t.UserId);

        // ステータスフィルタ付き一覧取得用複合インデックス
        builder.HasIndex(t => new { t.UserId, t.Status });

        builder.HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
