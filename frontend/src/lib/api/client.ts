import axios, { AxiosError, InternalAxiosRequestConfig } from "axios";
import type { ApiErrorResponse, AuthResponse } from "@/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

/** API クライアント（認証不要エンドポイント向け） */
export const publicClient = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

/** API クライアント（認証必要エンドポイント向け） */
export const authClient = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

/** アクセストークンをメモリに保持する（XSS 対策として localStorage を使わない） */
let accessToken: string | null = null;

/** アクセストークンを設定する */
export function setAccessToken(token: string | null): void {
  accessToken = token;
}

/** アクセストークンを取得する */
export function getAccessToken(): string | null {
  return accessToken;
}

// リクエストインターセプター: Authorization ヘッダーを付与
authClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

// トークンリフレッシュの重複実行を防ぐ
let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

/**
 * リフレッシュ完了を待機中のリクエストに新しいトークンを通知する
 */
function onRefreshed(newToken: string): void {
  refreshSubscribers.forEach((callback) => callback(newToken));
  refreshSubscribers = [];
}

/**
 * リフレッシュ完了を待機するリクエストを登録する
 */
function addRefreshSubscriber(callback: (token: string) => void): void {
  refreshSubscribers.push(callback);
}

// レスポンスインターセプター: 401 時にトークンリフレッシュを試行
authClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    if (isRefreshing) {
      // 他のリクエストがリフレッシュ中なら完了を待つ
      return new Promise((resolve) => {
        addRefreshSubscriber((newToken: string) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          resolve(authClient(originalRequest));
        });
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      const response = await publicClient.post<AuthResponse>("/auth/refresh");
      const newToken = response.data.accessToken;
      setAccessToken(newToken);
      onRefreshed(newToken);
      originalRequest.headers.Authorization = `Bearer ${newToken}`;
      return authClient(originalRequest);
    } catch {
      // リフレッシュ失敗時はトークンをクリアしてログイン画面へ
      setAccessToken(null);
      refreshSubscribers = [];
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      return Promise.reject(error);
    } finally {
      isRefreshing = false;
    }
  }
);

/**
 * AxiosError から API エラーレスポンスを抽出する
 */
export function extractApiError(error: unknown): ApiErrorResponse | null {
  if (axios.isAxiosError(error) && error.response?.data?.error) {
    return error.response.data as ApiErrorResponse;
  }
  return null;
}

/**
 * エラーからユーザー向けメッセージを取得する
 */
export function getErrorMessage(error: unknown): string {
  const apiError = extractApiError(error);
  if (apiError) {
    return apiError.error.message;
  }
  if (error instanceof Error) {
    return error.message;
  }
  return "予期しないエラーが発生しました";
}
