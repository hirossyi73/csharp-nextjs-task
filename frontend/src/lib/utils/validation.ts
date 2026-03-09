/** メールアドレスの形式を検証する */
export function validateEmail(email: string): string | null {
  if (!email) return "メールアドレスは必須です";
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) return "メールアドレスの形式が正しくありません";
  return null;
}

/** パスワードの要件を検証する */
export function validatePassword(password: string): string | null {
  if (!password) return "パスワードは必須です";
  if (password.length < 8) return "パスワードは8文字以上で入力してください";
  if (!/[a-z]/.test(password)) return "パスワードには小文字を含めてください";
  if (!/[A-Z]/.test(password)) return "パスワードには大文字を含めてください";
  if (!/\d/.test(password)) return "パスワードには数字を含めてください";
  return null;
}

/** パスワード確認の一致を検証する */
export function validatePasswordConfirmation(
  password: string,
  confirmation: string
): string | null {
  if (!confirmation) return "パスワード確認は必須です";
  if (password !== confirmation) return "パスワードが一致しません";
  return null;
}

/** タスクタイトルを検証する */
export function validateTaskTitle(title: string): string | null {
  if (!title.trim()) return "タイトルは必須です";
  if (title.length > 200) return "タイトルは200文字以内で入力してください";
  return null;
}
