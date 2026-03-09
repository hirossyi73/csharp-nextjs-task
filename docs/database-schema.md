# データベース設計書

## 概要

- **DBMS**: PostgreSQL
- **ORM**: Entity Framework Core 10.0
- **命名規約**: テーブル名・列名は snake_case
- **主キー**: UUID（`gen_random_uuid()` によるサーバー側生成）

---

## ER 図（テキスト）

```
users 1──N tasks
users 1──N refresh_tokens
users 1──N email_verification_tokens
users 1──N password_reset_tokens
```

---

## テーブル定義

### users

ユーザーアカウント情報を管理する。

| 列名 | 型 | 制約 | デフォルト | 説明 |
|---|---|---|---|---|
| id | uuid | PK, NOT NULL | `gen_random_uuid()` | ユーザー ID |
| email | varchar(255) | NOT NULL, UNIQUE | - | メールアドレス |
| password_hash | varchar(255) | NOT NULL | - | BCrypt ハッシュ化パスワード |
| is_email_verified | boolean | NOT NULL | `false` | メール確認済みフラグ |
| created_at | timestamptz | NOT NULL | `NOW()` | 作成日時 |
| updated_at | timestamptz | NOT NULL | `NOW()` | 更新日時 |

**インデックス**:
- `IX_users_email` (UNIQUE) — email

---

### tasks

ユーザーのタスク情報を管理する。

| 列名 | 型 | 制約 | デフォルト | 説明 |
|---|---|---|---|---|
| id | uuid | PK, NOT NULL | `gen_random_uuid()` | タスク ID |
| user_id | uuid | FK → users.id, NOT NULL | - | 所有ユーザー ID |
| title | varchar(200) | NOT NULL | - | タスクタイトル |
| description | text | NULL 許容 | - | タスク説明 |
| status | smallint | NOT NULL | `0` | ステータス（0: NOT_STARTED, 1: IN_PROGRESS, 2: COMPLETED） |
| created_at | timestamptz | NOT NULL | `NOW()` | 作成日時 |
| updated_at | timestamptz | NOT NULL | `NOW()` | 更新日時 |

**外部キー**:
- `FK_tasks_users_user_id` → users.id (CASCADE DELETE)

**インデックス**:
- `IX_tasks_user_id` — user_id
- `IX_tasks_user_id_status` — (user_id, status) ※ フィルタ検索の高速化

---

### refresh_tokens

JWT リフレッシュトークンを管理する。トークンローテーション方式で運用。

| 列名 | 型 | 制約 | デフォルト | 説明 |
|---|---|---|---|---|
| id | uuid | PK, NOT NULL | `gen_random_uuid()` | トークン ID |
| user_id | uuid | FK → users.id, NOT NULL | - | ユーザー ID |
| token_hash | varchar(255) | NOT NULL, UNIQUE | - | SHA-256 ハッシュ化トークン |
| expires_at | timestamptz | NOT NULL | - | 有効期限 |
| is_revoked | boolean | NOT NULL | `false` | 無効化フラグ |
| created_at | timestamptz | NOT NULL | `NOW()` | 作成日時 |

**外部キー**:
- `FK_refresh_tokens_users_user_id` → users.id (CASCADE DELETE)

**インデックス**:
- `IX_refresh_tokens_token_hash` (UNIQUE) — token_hash
- `IX_refresh_tokens_user_id` — user_id

---

### email_verification_tokens

メール確認トークンを管理する。ユーザー仮登録時に発行。

| 列名 | 型 | 制約 | デフォルト | 説明 |
|---|---|---|---|---|
| id | uuid | PK, NOT NULL | `gen_random_uuid()` | トークン ID |
| user_id | uuid | FK → users.id, NOT NULL | - | ユーザー ID |
| token_hash | varchar(255) | NOT NULL, UNIQUE | - | SHA-256 ハッシュ化トークン |
| expires_at | timestamptz | NOT NULL | - | 有効期限 |
| is_used | boolean | NOT NULL | `false` | 使用済みフラグ |
| created_at | timestamptz | NOT NULL | `NOW()` | 作成日時 |

**外部キー**:
- `FK_email_verification_tokens_users_user_id` → users.id (CASCADE DELETE)

**インデックス**:
- `IX_email_verification_tokens_token_hash` (UNIQUE) — token_hash
- `IX_email_verification_tokens_user_id` — user_id

---

### password_reset_tokens

パスワードリセットトークンを管理する。

| 列名 | 型 | 制約 | デフォルト | 説明 |
|---|---|---|---|---|
| id | uuid | PK, NOT NULL | `gen_random_uuid()` | トークン ID |
| user_id | uuid | FK → users.id, NOT NULL | - | ユーザー ID |
| token_hash | varchar(255) | NOT NULL, UNIQUE | - | SHA-256 ハッシュ化トークン |
| expires_at | timestamptz | NOT NULL | - | 有効期限 |
| is_used | boolean | NOT NULL | `false` | 使用済みフラグ |
| created_at | timestamptz | NOT NULL | `NOW()` | 作成日時 |

**外部キー**:
- `FK_password_reset_tokens_users_user_id` → users.id (CASCADE DELETE)

**インデックス**:
- `IX_password_reset_tokens_token_hash` (UNIQUE) — token_hash
- `IX_password_reset_tokens_user_id` — user_id

---

## 設計方針

### セキュリティ

- パスワードは BCrypt でハッシュ化して保存（平文保存禁止）
- トークン（リフレッシュ、メール確認、パスワードリセット）は SHA-256 ハッシュで保存
- 全トークンに有効期限を設定し、期限切れトークンは無効扱い

### データ整合性

- 全外部キーに CASCADE DELETE を設定（ユーザー削除時に関連データを自動削除）
- email 列に UNIQUE 制約でメールアドレスの重複を防止
- token_hash 列に UNIQUE 制約でトークンの重複を防止

### パフォーマンス

- tasks テーブルに (user_id, status) 複合インデックスを設定し、ユーザー別ステータスフィルタの検索を高速化
- トークン検索用に token_hash の UNIQUE インデックスを活用
