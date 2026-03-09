/** ユーザー登録リクエスト（メアドのみで仮登録） */
export interface RegisterRequest {
  email: string;
}

/** ログインリクエスト */
export interface LoginRequest {
  email: string;
  password: string;
}

/** メール確認リクエスト（パスワード設定で本登録） */
export interface VerifyEmailRequest {
  token: string;
  password: string;
  passwordConfirmation: string;
}

/** パスワードリセット要求リクエスト */
export interface ForgotPasswordRequest {
  email: string;
}

/** パスワード再設定リクエスト */
export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  newPasswordConfirmation: string;
}

/** 認証レスポンス（ログイン・トークンリフレッシュ） */
export interface AuthResponse {
  accessToken: string;
  expiresIn: number;
}

/** メッセージレスポンス */
export interface MessageResponse {
  message: string;
}
