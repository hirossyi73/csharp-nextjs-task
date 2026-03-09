import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import TaskCreateDialog from "../tasks/TaskCreateDialog";

describe("TaskCreateDialog", () => {
  const onClose = jest.fn();
  const onSubmit = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("ダイアログが開いているときフォームを表示する", () => {
    render(
      <TaskCreateDialog open={true} onClose={onClose} onSubmit={onSubmit} />
    );

    expect(screen.getByText("タスク作成")).toBeInTheDocument();
    expect(screen.getByLabelText("タイトル")).toBeInTheDocument();
    expect(screen.getByLabelText("説明（任意）")).toBeInTheDocument();
  });

  it("タイトル未入力でバリデーションエラーを表示する", async () => {
    const user = userEvent.setup();
    render(
      <TaskCreateDialog open={true} onClose={onClose} onSubmit={onSubmit} />
    );

    await user.click(screen.getByRole("button", { name: "作成" }));

    expect(await screen.findByText("タイトルは必須です")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("正常な入力で onSubmit が呼ばれる", async () => {
    onSubmit.mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(
      <TaskCreateDialog open={true} onClose={onClose} onSubmit={onSubmit} />
    );

    await user.type(screen.getByLabelText("タイトル"), "新しいタスク");
    await user.type(screen.getByLabelText("説明（任意）"), "タスクの説明");
    await user.click(screen.getByRole("button", { name: "作成" }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith({
        title: "新しいタスク",
        description: "タスクの説明",
      });
    });
  });

  it("キャンセルボタンで onClose が呼ばれる", async () => {
    const user = userEvent.setup();
    render(
      <TaskCreateDialog open={true} onClose={onClose} onSubmit={onSubmit} />
    );

    await user.click(screen.getByRole("button", { name: "キャンセル" }));

    expect(onClose).toHaveBeenCalled();
  });

  it("送信失敗でエラーメッセージを表示する", async () => {
    onSubmit.mockRejectedValue(new Error("作成に失敗しました"));
    const user = userEvent.setup();
    render(
      <TaskCreateDialog open={true} onClose={onClose} onSubmit={onSubmit} />
    );

    await user.type(screen.getByLabelText("タイトル"), "新しいタスク");
    await user.click(screen.getByRole("button", { name: "作成" }));

    expect(await screen.findByText("作成に失敗しました")).toBeInTheDocument();
  });
});
