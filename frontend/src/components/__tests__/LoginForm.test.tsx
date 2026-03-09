import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import LoginForm from "../auth/LoginForm";

const mockPush = jest.fn();
jest.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush }),
}));

const mockLogin = jest.fn();
jest.mock("@/hooks/useAuth", () => ({
  useAuth: () => ({
    login: mockLogin,
    isAuthenticated: false,
    isLoading: false,
    register: jest.fn(),
    logout: jest.fn(),
  }),
}));

describe("LoginForm", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("ログインフォームを表示する", () => {
    render(<LoginForm />);

    expect(screen.getByRole("heading", { name: "ログイン" })).toBeInTheDocument();
    expect(screen.getByLabelText("メールアドレス")).toBeInTheDocument();
    expect(screen.getByLabelText("パスワード")).toBeInTheDocument();
  });

  it("メールアドレス未入力でバリデーションエラーを表示する", async () => {
    const user = userEvent.setup();
    render(<LoginForm />);

    await user.click(screen.getByRole("button", { name: "ログイン" }));

    expect(await screen.findByText("メールアドレスは必須です")).toBeInTheDocument();
    expect(mockLogin).not.toHaveBeenCalled();
  });

  it("パスワード未入力でバリデーションエラーを表示する", async () => {
    const user = userEvent.setup();
    render(<LoginForm />);

    await user.type(screen.getByLabelText("メールアドレス"), "test@example.com");
    await user.click(screen.getByRole("button", { name: "ログイン" }));

    expect(await screen.findByText("パスワードは必須です")).toBeInTheDocument();
    expect(mockLogin).not.toHaveBeenCalled();
  });

  it("正常なログインで /tasks にリダイレクトする", async () => {
    mockLogin.mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(<LoginForm />);

    await user.type(screen.getByLabelText("メールアドレス"), "test@example.com");
    await user.type(screen.getByLabelText("パスワード"), "Password123");
    await user.click(screen.getByRole("button", { name: "ログイン" }));

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: "test@example.com",
        password: "Password123",
      });
      expect(mockPush).toHaveBeenCalledWith("/tasks");
    });
  });

  it("ログイン失敗でエラーメッセージを表示する", async () => {
    mockLogin.mockRejectedValue(new Error("認証に失敗しました"));
    const user = userEvent.setup();
    render(<LoginForm />);

    await user.type(screen.getByLabelText("メールアドレス"), "test@example.com");
    await user.type(screen.getByLabelText("パスワード"), "Password123");
    await user.click(screen.getByRole("button", { name: "ログイン" }));

    expect(await screen.findByText("認証に失敗しました")).toBeInTheDocument();
  });

  it("登録リンクが存在する", () => {
    render(<LoginForm />);

    expect(screen.getByText("アカウントを作成")).toBeInTheDocument();
  });
});
