# TaskFlow

タスク管理アプリケーション。ユーザー認証付きの CRUD 機能を提供する。

## 技術スタック

| レイヤー | 技術 |
|---|---|
| フロントエンド | Next.js 15, React 19, TypeScript, MUI 6 |
| バックエンド | ASP.NET Core (.NET 10), C# |
| データベース | PostgreSQL 16 |
| ORM | Entity Framework Core 10 |
| メール | Mailpit（開発環境） |
| テスト（BE） | xUnit v3, Moq, FluentAssertions |
| テスト（FE） | Jest 30, React Testing Library |

## アーキテクチャ

バックエンドはクリーンアーキテクチャを採用。

```
backend/src/
  TaskFlow.Domain/          # エンティティ、列挙型
  TaskFlow.Application/     # サービス、インタフェース、Result パターン
  TaskFlow.Infrastructure/  # DB、JWT、メール、リポジトリ実装
  TaskFlow.Api/             # コントローラー、DTO、ミドルウェア
```

## セットアップ

### 前提条件

- Docker / Docker Compose
- Node.js 22+（フロントエンド開発時）

### 起動

```bash
docker compose up -d
```

| サービス | URL |
|---|---|
| フロントエンド | http://localhost:3000 |
| バックエンド API | http://localhost:5000 |
| Mailpit（メール確認） | http://localhost:8025 |

ポート 5000 が使用中の場合:

```bash
BACKEND_PORT=5001 docker compose up -d
```

### 停止

```bash
docker compose down
```

データベースのデータを削除する場合:

```bash
docker compose down -v
```

## テスト

### バックエンド

```bash
docker run --rm -v $(pwd)/backend:/app -w /app \
  mcr.microsoft.com/dotnet/sdk:10.0-preview \
  dotnet run --project tests/TaskFlow.Tests/TaskFlow.Tests.csproj
```

### フロントエンド

```bash
cd frontend
npm test
```

## API ドキュメント

- [OpenAPI 仕様 (swagger.yaml)](docs/api/swagger.yaml)
- [DB 設計書](docs/database-schema.md)

## 主な機能

- ユーザー登録（メール確認付き）
- ログイン / ログアウト（JWT + リフレッシュトークン）
- パスワードリセット
- タスク CRUD（作成・一覧・詳細・更新・削除）
- タスクステータス管理（未着手 / 進行中 / 完了）
- ページネーション / ステータスフィルタ

## 環境変数

主要な環境変数は `docker-compose.yml` にデフォルト値が設定されている。本番環境では以下を必ず変更すること:

| 変数 | 説明 | デフォルト |
|---|---|---|
| `JWT_SECRET_KEY` | JWT 署名キー | 開発用の固定値 |
| `POSTGRES_PASSWORD` | DB パスワード | `taskflow_pass` |
| `ASPNETCORE_ENVIRONMENT` | 実行環境 | `Development` |
