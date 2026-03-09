import { renderHook, act, waitFor } from "@testing-library/react";
import { useTasks } from "../useTasks";
import * as tasksApi from "@/lib/api/tasks";
import type { TaskResponse, TaskListResponse } from "@/types";

jest.mock("@/lib/api/tasks");

const mockedTasksApi = tasksApi as jest.Mocked<typeof tasksApi>;

const mockTask: TaskResponse = {
  id: "task-1",
  title: "テストタスク",
  description: "テスト説明",
  status: "NOT_STARTED",
  createdAt: "2026-01-01T00:00:00Z",
  updatedAt: "2026-01-01T00:00:00Z",
};

const mockTaskListResponse: TaskListResponse = {
  items: [mockTask],
  pagination: {
    currentPage: 1,
    perPage: 20,
    totalCount: 1,
    totalPages: 1,
  },
};

describe("useTasks", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedTasksApi.getTasks.mockResolvedValue(mockTaskListResponse);
  });

  it("初回レンダリングでタスク一覧を取得する", async () => {
    const { result } = renderHook(() => useTasks());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.tasks).toEqual([mockTask]);
    expect(result.current.pagination).toEqual(mockTaskListResponse.pagination);
    expect(mockedTasksApi.getTasks).toHaveBeenCalledWith({
      page: 1,
      perPage: 20,
      status: undefined,
    });
  });

  it("取得失敗時にエラーメッセージを設定する", async () => {
    mockedTasksApi.getTasks.mockRejectedValue(new Error("Network Error"));

    const { result } = renderHook(() => useTasks());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBe("タスクの取得に失敗しました");
    expect(result.current.tasks).toEqual([]);
  });

  it("createTask でタスクを作成し一覧をリロードする", async () => {
    const newTask: TaskResponse = {
      ...mockTask,
      id: "task-2",
      title: "新規タスク",
    };
    mockedTasksApi.createTask.mockResolvedValue(newTask);

    const { result } = renderHook(() => useTasks());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    let created: TaskResponse | undefined;
    await act(async () => {
      created = await result.current.createTask({
        title: "新規タスク",
      });
    });

    expect(created).toEqual(newTask);
    expect(mockedTasksApi.createTask).toHaveBeenCalledWith({
      title: "新規タスク",
    });
    // 作成後にリロードが走る
    expect(mockedTasksApi.getTasks).toHaveBeenCalledTimes(2);
  });

  it("updateTask でタスクを更新しローカル状態を反映する", async () => {
    const updatedTask: TaskResponse = {
      ...mockTask,
      title: "更新済みタスク",
      status: "IN_PROGRESS",
    };
    mockedTasksApi.updateTask.mockResolvedValue(updatedTask);

    const { result } = renderHook(() => useTasks());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    await act(async () => {
      await result.current.updateTask("task-1", {
        title: "更新済みタスク",
        status: "IN_PROGRESS",
      });
    });

    expect(result.current.tasks[0].title).toBe("更新済みタスク");
    expect(result.current.tasks[0].status).toBe("IN_PROGRESS");
  });

  it("deleteTask でタスクを削除しリロードする", async () => {
    mockedTasksApi.deleteTask.mockResolvedValue();

    const { result } = renderHook(() => useTasks());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    await act(async () => {
      await result.current.deleteTask("task-1");
    });

    expect(mockedTasksApi.deleteTask).toHaveBeenCalledWith("task-1");
    // 削除後にリロードが走る
    expect(mockedTasksApi.getTasks).toHaveBeenCalledTimes(2);
  });

  it("setStatusFilter でフィルタ変更時にページが1にリセットされる", async () => {
    const { result } = renderHook(() => useTasks());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    // ページを2に変更
    await act(async () => {
      result.current.setPage(2);
    });

    await waitFor(() => {
      expect(result.current.page).toBe(2);
    });

    // フィルタ変更でページが1にリセットされる
    await act(async () => {
      result.current.setStatusFilter("COMPLETED");
    });

    await waitFor(() => {
      expect(result.current.page).toBe(1);
      expect(result.current.statusFilter).toBe("COMPLETED");
    });
  });

  it("reload で手動リロードできる", async () => {
    const { result } = renderHook(() => useTasks());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(mockedTasksApi.getTasks).toHaveBeenCalledTimes(1);

    await act(async () => {
      await result.current.reload();
    });

    expect(mockedTasksApi.getTasks).toHaveBeenCalledTimes(2);
  });
});
