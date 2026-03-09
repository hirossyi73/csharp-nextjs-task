import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import RegisterForm from "../auth/RegisterForm";

const mockRegister = jest.fn();
jest.mock("@/hooks/useAuth", () => ({
  useAuth: () => ({
    register: mockRegister,
    login: jest.fn(),
    logout: jest.fn(),
    isAuthenticated: false,
    isLoading: false,
  }),
}));

describe("RegisterForm", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("登録フォームを表示する", () => {
    render(<RegisterForm />);

    expect(screen.getByText("アカウント作成")).toBeInTheDocument();
    expect(screen.getByLabelText("メールアドレス")).toBeInTheDocument();
    expect(screen.getByLabelText("パスワード")).toBeInTheDocument();
    expect(screen.getByLabelText("パスワード確認")).toBeInTheDocument();
  });

  it("全フィールド未入力でバリデーションエラーを表示する", async () => {
    const user = userEvent.setup();
    render(<RegisterForm />);

    await user.click(screen.getByRole("button", { name: "登録" }));

    expect(await screen.findByText("メールアドレスは必須です")).toBeInTheDocument();
    expect(mockRegister).not.toHaveBeenCalled();
  });

  it("パスワード不一致でバリデーションエラーを表示する", async () => {
    const user = userEvent.setup();
    render(<RegisterForm />);

    await user.type(screen.getByLabelText("メールアドレス"), "test@example.com");
    await user.type(screen.getByLabelText("パスワード"), "Password123");
    await user.type(screen.getByLabelText("パスワード確認"), "Different123");
    await user.click(screen.getByRole("button", { name: "登録" }));

    expect(await screen.findByText("パスワードが一致しません")).toBeInTheDocument();
    expect(mockRegister).not.toHaveBeenCalled();
  });

  it("正常な登録で成功メッセージを表示する", async () => {
    mockRegister.mockResolvedValue("確認メールを送信しました");
    const user = userEvent.setup();
    render(<RegisterForm />);

    await user.type(screen.getByLabelText("メールアドレス"), "test@example.com");
    await user.type(screen.getByLabelText("パスワード"), "Password123");
    await user.type(screen.getByLabelText("パスワード確認"), "Password123");
    await user.click(screen.getByRole("button", { name: "登録" }));

    await waitFor(() => {
      expect(screen.getByText("確認メールを送信しました")).toBeInTheDocument();
      expect(screen.getByText("登録完了")).toBeInTheDocument();
    });
  });

  it("登録失敗でエラーメッセージを表示する", async () => {
    mockRegister.mockRejectedValue(new Error("メールアドレスは既に使用されています"));
    const user = userEvent.setup();
    render(<RegisterForm />);

    await user.type(screen.getByLabelText("メールアドレス"), "test@example.com");
    await user.type(screen.getByLabelText("パスワード"), "Password123");
    await user.type(screen.getByLabelText("パスワード確認"), "Password123");
    await user.click(screen.getByRole("button", { name: "登録" }));

    expect(
      await screen.findByText("メールアドレスは既に使用されています")
    ).toBeInTheDocument();
  });

  it("ログインリンクが存在する", () => {
    render(<RegisterForm />);

    expect(screen.getByText("すでにアカウントをお持ちの方")).toBeInTheDocument();
  });
});
