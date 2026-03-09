import { renderHook, act } from "@testing-library/react";
import { type ReactNode } from "react";
import { useAuth } from "../useAuth";
import { AuthContext, type AuthContextValue } from "@/contexts/AuthContext";

function createWrapper(value: AuthContextValue) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
    );
  };
}

const mockContextValue: AuthContextValue = {
  isAuthenticated: false,
  isLoading: false,
  login: jest.fn(),
  register: jest.fn(),
  logout: jest.fn(),
};

describe("useAuth", () => {
  it("AuthProvider 内でコンテキスト値を返す", () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(mockContextValue),
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.isLoading).toBe(false);
    expect(result.current.login).toBeDefined();
    expect(result.current.register).toBeDefined();
    expect(result.current.logout).toBeDefined();
  });

  it("認証済み状態を正しく返す", () => {
    const authenticatedValue: AuthContextValue = {
      ...mockContextValue,
      isAuthenticated: true,
    };

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(authenticatedValue),
    });

    expect(result.current.isAuthenticated).toBe(true);
  });

  it("AuthProvider の外で使用すると例外をスローする", () => {
    // renderHook のエラー出力を抑制
    const consoleSpy = jest.spyOn(console, "error").mockImplementation();

    expect(() => {
      renderHook(() => useAuth());
    }).toThrow("useAuth は AuthProvider 内で使用してください");

    consoleSpy.mockRestore();
  });

  it("login を呼び出せる", async () => {
    const loginMock = jest.fn().mockResolvedValue(undefined);
    const value: AuthContextValue = { ...mockContextValue, login: loginMock };

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(value),
    });

    await act(async () => {
      await result.current.login({
        email: "test@example.com",
        password: "password123",
      });
    });

    expect(loginMock).toHaveBeenCalledWith({
      email: "test@example.com",
      password: "password123",
    });
  });

  it("logout を呼び出せる", async () => {
    const logoutMock = jest.fn().mockResolvedValue(undefined);
    const value: AuthContextValue = { ...mockContextValue, logout: logoutMock };

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(value),
    });

    await act(async () => {
      await result.current.logout();
    });

    expect(logoutMock).toHaveBeenCalled();
  });
});
