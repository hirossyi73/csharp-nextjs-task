/** ユーザー登録リクエスト */
export interface RegisterRequest {
  email: string;
  password: string;
  passwordConfirmation: string;
}

/** ログインリクエスト */
export interface LoginRequest {
  email: string;
  password: string;
}

/** メール確認リクエスト */
export interface VerifyEmailRequest {
  token: string;
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
