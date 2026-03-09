"use client";

import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import * as authApi from "@/lib/api/auth";
import { getAccessToken } from "@/lib/api/client";
import type { LoginRequest } from "@/types";

/** 認証状態 */
export interface AuthState {
  /** 認証済みかどうか */
  isAuthenticated: boolean;
  /** 認証状態の初期化が完了したかどうか */
  isLoading: boolean;
}

/** AuthContext が提供する値 */
export interface AuthContextValue extends AuthState {
  /** ログイン */
  login: (data: LoginRequest) => Promise<void>;
  /** ユーザー登録 */
  register: (email: string) => Promise<string>;
  /** ログアウト */
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

/** 認証状態を管理するプロバイダー */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  // 初回マウント時にリフレッシュトークンでセッション復元を試みる
  useEffect(() => {
    const initAuth = async () => {
      try {
        await authApi.refreshToken();
        setIsAuthenticated(true);
      } catch {
        setIsAuthenticated(false);
      } finally {
        setIsLoading(false);
      }
    };
    initAuth();
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    await authApi.login(data);
    setIsAuthenticated(true);
  }, []);

  const register = useCallback(async (email: string) => {
    const response = await authApi.register({ email });
    return response.message;
  }, []);

  const logout = useCallback(async () => {
    try {
      // トークンがある場合のみサーバーに通知
      if (getAccessToken()) {
        await authApi.logout();
      }
    } finally {
      setIsAuthenticated(false);
    }
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ isAuthenticated, isLoading, login, register, logout }),
    [isAuthenticated, isLoading, login, register, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
