import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import TaskEditDialog from "../tasks/TaskEditDialog";
import type { TaskResponse } from "@/types";

const mockTask: TaskResponse = {
  id: "task-1",
  title: "既存タスク",
  description: "既存の説明",
  status: "NOT_STARTED",
  createdAt: "2026-01-01T00:00:00Z",
  updatedAt: "2026-01-01T00:00:00Z",
};

describe("TaskEditDialog", () => {
  const onClose = jest.fn();
  const onSubmit = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("タスクの値が初期値として表示される", () => {
    render(
      <TaskEditDialog
        open={true}
        task={mockTask}
        onClose={onClose}
        onSubmit={onSubmit}
      />
    );

    expect(screen.getByText("タスク編集")).toBeInTheDocument();
    expect(screen.getByDisplayValue("既存タスク")).toBeInTheDocument();
    expect(screen.getByDisplayValue("既存の説明")).toBeInTheDocument();
  });

  it("タイトルを空にするとバリデーションエラーを表示する", async () => {
    const user = userEvent.setup();
    render(
      <TaskEditDialog
        open={true}
        task={mockTask}
        onClose={onClose}
        onSubmit={onSubmit}
      />
    );

    const titleInput = screen.getByDisplayValue("既存タスク");
    await user.clear(titleInput);
    await user.click(screen.getByRole("button", { name: "更新" }));

    expect(await screen.findByText("タイトルは必須です")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("正常な更新で onSubmit が呼ばれる", async () => {
    onSubmit.mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(
      <TaskEditDialog
        open={true}
        task={mockTask}
        onClose={onClose}
        onSubmit={onSubmit}
      />
    );

    const titleInput = screen.getByDisplayValue("既存タスク");
    await user.clear(titleInput);
    await user.type(titleInput, "更新後タスク");
    await user.click(screen.getByRole("button", { name: "更新" }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith("task-1", {
        title: "更新後タスク",
        description: "既存の説明",
        status: "NOT_STARTED",
      });
    });
  });

  it("キャンセルボタンで onClose が呼ばれる", async () => {
    const user = userEvent.setup();
    render(
      <TaskEditDialog
        open={true}
        task={mockTask}
        onClose={onClose}
        onSubmit={onSubmit}
      />
    );

    await user.click(screen.getByRole("button", { name: "キャンセル" }));

    expect(onClose).toHaveBeenCalled();
  });

  it("送信失敗でエラーメッセージを表示する", async () => {
    onSubmit.mockRejectedValue(new Error("更新に失敗しました"));
    const user = userEvent.setup();
    render(
      <TaskEditDialog
        open={true}
        task={mockTask}
        onClose={onClose}
        onSubmit={onSubmit}
      />
    );

    await user.click(screen.getByRole("button", { name: "更新" }));

    expect(await screen.findByText("更新に失敗しました")).toBeInTheDocument();
  });
});
