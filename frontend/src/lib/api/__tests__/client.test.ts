import axios, { AxiosError } from "axios";
import {
  setAccessToken,
  getAccessToken,
  extractApiError,
  getErrorMessage,
} from "../client";

describe("setAccessToken / getAccessToken", () => {
  afterEach(() => {
    setAccessToken(null);
  });

  it("トークンを設定・取得できる", () => {
    setAccessToken("test-token");
    expect(getAccessToken()).toBe("test-token");
  });

  it("null を設定するとトークンがクリアされる", () => {
    setAccessToken("test-token");
    setAccessToken(null);
    expect(getAccessToken()).toBeNull();
  });
});

describe("extractApiError", () => {
  it("API エラーレスポンスを抽出できる", () => {
    const error = new AxiosError("test", "ERR", undefined, undefined, {
      data: {
        error: {
          code: "VALIDATION_ERROR",
          message: "入力エラー",
        },
      },
      status: 422,
      statusText: "Unprocessable Entity",
      headers: {},
      config: { headers: new axios.AxiosHeaders() },
    });

    const result = extractApiError(error);
    expect(result?.error.code).toBe("VALIDATION_ERROR");
    expect(result?.error.message).toBe("入力エラー");
  });

  it("非 Axios エラーで null が返される", () => {
    expect(extractApiError(new Error("test"))).toBeNull();
  });

  it("レスポンスなしで null が返される", () => {
    const error = new AxiosError("test");
    expect(extractApiError(error)).toBeNull();
  });
});

describe("getErrorMessage", () => {
  it("API エラーからメッセージを取得できる", () => {
    const error = new AxiosError("test", "ERR", undefined, undefined, {
      data: {
        error: {
          code: "AUTH_CREDENTIALS_INVALID",
          message: "認証情報が不正です",
        },
      },
      status: 401,
      statusText: "Unauthorized",
      headers: {},
      config: { headers: new axios.AxiosHeaders() },
    });

    expect(getErrorMessage(error)).toBe("認証情報が不正です");
  });

  it("一般的な Error からメッセージを取得できる", () => {
    expect(getErrorMessage(new Error("テストエラー"))).toBe("テストエラー");
  });

  it("不明なエラーでデフォルトメッセージが返される", () => {
    expect(getErrorMessage("unknown")).toBe("予期しないエラーが発生しました");
  });
});
