import {
  validateEmail,
  validatePassword,
  validatePasswordConfirmation,
  validateTaskTitle,
} from "../validation";

describe("validateEmail", () => {
  it("空文字でエラーが返される", () => {
    expect(validateEmail("")).toBe("メールアドレスは必須です");
  });

  it("不正な形式でエラーが返される", () => {
    expect(validateEmail("invalid")).toBe(
      "メールアドレスの形式が正しくありません"
    );
  });

  it("有効なメールアドレスで null が返される", () => {
    expect(validateEmail("test@example.com")).toBeNull();
  });
});

describe("validatePassword", () => {
  it("空文字でエラーが返される", () => {
    expect(validatePassword("")).toBe("パスワードは必須です");
  });

  it("8文字未満でエラーが返される", () => {
    expect(validatePassword("Abc1")).toBe(
      "パスワードは8文字以上で入力してください"
    );
  });

  it("小文字なしでエラーが返される", () => {
    expect(validatePassword("PASSWORD1")).toBe(
      "パスワードには小文字を含めてください"
    );
  });

  it("大文字なしでエラーが返される", () => {
    expect(validatePassword("password1")).toBe(
      "パスワードには大文字を含めてください"
    );
  });

  it("数字なしでエラーが返される", () => {
    expect(validatePassword("Passwordd")).toBe(
      "パスワードには数字を含めてください"
    );
  });

  it("有効なパスワードで null が返される", () => {
    expect(validatePassword("Password123")).toBeNull();
  });
});

describe("validatePasswordConfirmation", () => {
  it("空文字でエラーが返される", () => {
    expect(validatePasswordConfirmation("Password123", "")).toBe(
      "パスワード確認は必須です"
    );
  });

  it("不一致でエラーが返される", () => {
    expect(validatePasswordConfirmation("Password123", "Different")).toBe(
      "パスワードが一致しません"
    );
  });

  it("一致で null が返される", () => {
    expect(
      validatePasswordConfirmation("Password123", "Password123")
    ).toBeNull();
  });
});

describe("validateTaskTitle", () => {
  it("空文字でエラーが返される", () => {
    expect(validateTaskTitle("")).toBe("タイトルは必須です");
  });

  it("空白のみでエラーが返される", () => {
    expect(validateTaskTitle("   ")).toBe("タイトルは必須です");
  });

  it("200文字超でエラーが返される", () => {
    expect(validateTaskTitle("a".repeat(201))).toBe(
      "タイトルは200文字以内で入力してください"
    );
  });

  it("有効なタイトルで null が返される", () => {
    expect(validateTaskTitle("テストタスク")).toBeNull();
  });

  it("200文字ちょうどで null が返される", () => {
    expect(validateTaskTitle("a".repeat(200))).toBeNull();
  });
});
