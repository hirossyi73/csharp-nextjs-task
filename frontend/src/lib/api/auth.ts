import { publicClient, authClient, setAccessToken } from "./client";
import type {
  RegisterRequest,
  LoginRequest,
  VerifyEmailRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  AuthResponse,
  MessageResponse,
} from "@/types";

/** ユーザー仮登録 */
export async function register(data: RegisterRequest): Promise<MessageResponse> {
  const response = await publicClient.post<MessageResponse>("/auth/register", data);
  return response.data;
}

/** メール確認（本登録） */
export async function verifyEmail(data: VerifyEmailRequest): Promise<MessageResponse> {
  const response = await publicClient.post<MessageResponse>("/auth/verify-email", data);
  return response.data;
}

/** ログイン */
export async function login(data: LoginRequest): Promise<AuthResponse> {
  const response = await publicClient.post<AuthResponse>("/auth/login", data);
  setAccessToken(response.data.accessToken);
  return response.data;
}

/** ログアウト */
export async function logout(): Promise<void> {
  await authClient.post("/auth/logout");
  setAccessToken(null);
}

/** トークンリフレッシュ */
export async function refreshToken(): Promise<AuthResponse> {
  const response = await publicClient.post<AuthResponse>("/auth/refresh");
  setAccessToken(response.data.accessToken);
  return response.data;
}

/** パスワードリセット要求 */
export async function forgotPassword(data: ForgotPasswordRequest): Promise<MessageResponse> {
  const response = await publicClient.post<MessageResponse>("/auth/forgot-password", data);
  return response.data;
}

/** パスワード再設定 */
export async function resetPassword(data: ResetPasswordRequest): Promise<MessageResponse> {
  const response = await publicClient.post<MessageResponse>("/auth/reset-password", data);
  return response.data;
}
