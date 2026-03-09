"use client";

import { useContext } from "react";
import { AuthContext, type AuthContextValue } from "@/contexts/AuthContext";

/**
 * 認証コンテキストにアクセスするカスタムフック
 *
 * AuthProvider の外で使用すると例外をスローする
 */
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth は AuthProvider 内で使用してください");
  }
  return context;
}
