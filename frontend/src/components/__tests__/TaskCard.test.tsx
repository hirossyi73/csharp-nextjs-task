import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import TaskCard from "../tasks/TaskCard";
import type { TaskResponse } from "@/types";

const mockTask: TaskResponse = {
  id: "task-1",
  title: "テストタスク",
  description: "タスクの説明文",
  status: "NOT_STARTED",
  createdAt: "2026-01-01T00:00:00Z",
  updatedAt: "2026-01-01T00:00:00Z",
};

describe("TaskCard", () => {
  const onEdit = jest.fn();
  const onDelete = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("タスクのタイトルと説明を表示する", () => {
    render(<TaskCard task={mockTask} onEdit={onEdit} onDelete={onDelete} />);

    expect(screen.getByText("テストタスク")).toBeInTheDocument();
    expect(screen.getByText("タスクの説明文")).toBeInTheDocument();
  });

  it("ステータス「未着手」を表示する", () => {
    render(<TaskCard task={mockTask} onEdit={onEdit} onDelete={onDelete} />);

    expect(screen.getByText("未着手")).toBeInTheDocument();
  });

  it("ステータス「進行中」を表示する", () => {
    const task = { ...mockTask, status: "IN_PROGRESS" as const };
    render(<TaskCard task={task} onEdit={onEdit} onDelete={onDelete} />);

    expect(screen.getByText("進行中")).toBeInTheDocument();
  });

  it("ステータス「完了」を表示する", () => {
    const task = { ...mockTask, status: "COMPLETED" as const };
    render(<TaskCard task={task} onEdit={onEdit} onDelete={onDelete} />);

    expect(screen.getByText("完了")).toBeInTheDocument();
  });

  it("説明がない場合は説明を表示しない", () => {
    const task = { ...mockTask, description: null };
    render(<TaskCard task={task} onEdit={onEdit} onDelete={onDelete} />);

    expect(screen.getByText("テストタスク")).toBeInTheDocument();
    expect(screen.queryByText("タスクの説明文")).not.toBeInTheDocument();
  });

  it("編集ボタンクリックで onEdit が呼ばれる", async () => {
    const user = userEvent.setup();
    render(<TaskCard task={mockTask} onEdit={onEdit} onDelete={onDelete} />);

    await user.click(screen.getByLabelText("編集"));

    expect(onEdit).toHaveBeenCalledWith(mockTask);
  });

  it("削除ボタンクリックで onDelete が呼ばれる", async () => {
    const user = userEvent.setup();
    render(<TaskCard task={mockTask} onEdit={onEdit} onDelete={onDelete} />);

    await user.click(screen.getByLabelText("削除"));

    expect(onDelete).toHaveBeenCalledWith(mockTask);
  });
});
