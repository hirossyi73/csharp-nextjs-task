"use client";

import { useCallback, useEffect, useState } from "react";
import * as tasksApi from "@/lib/api/tasks";
import type {
  TaskResponse,
  TaskStatus,
  PaginationResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
} from "@/types";

/** useTasks フックの戻り値 */
export interface UseTasksReturn {
  tasks: TaskResponse[];
  pagination: PaginationResponse | null;
  isLoading: boolean;
  error: string | null;
  statusFilter: TaskStatus | undefined;
  setStatusFilter: (status: TaskStatus | undefined) => void;
  page: number;
  setPage: (page: number) => void;
  reload: () => Promise<void>;
  createTask: (data: CreateTaskRequest) => Promise<TaskResponse>;
  updateTask: (id: string, data: UpdateTaskRequest) => Promise<TaskResponse>;
  deleteTask: (id: string) => Promise<void>;
}

/**
 * タスク CRUD とページネーション・フィルタリングを管理するカスタムフック
 */
export function useTasks(): UseTasksReturn {
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [pagination, setPagination] = useState<PaginationResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<TaskStatus | undefined>();
  const [page, setPage] = useState(1);

  /** タスク一覧を取得する */
  const fetchTasks = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await tasksApi.getTasks({
        page,
        perPage: 20,
        status: statusFilter,
      });
      setTasks(result.items);
      setPagination(result.pagination);
    } catch {
      setError("タスクの取得に失敗しました");
    } finally {
      setIsLoading(false);
    }
  }, [page, statusFilter]);

  useEffect(() => {
    fetchTasks();
  }, [fetchTasks]);

  // フィルター変更時はページを1に戻す
  const handleSetStatusFilter = useCallback(
    (status: TaskStatus | undefined) => {
      setStatusFilter(status);
      setPage(1);
    },
    []
  );

  /** タスクを作成する */
  const createTask = useCallback(
    async (data: CreateTaskRequest): Promise<TaskResponse> => {
      const created = await tasksApi.createTask(data);
      await fetchTasks();
      return created;
    },
    [fetchTasks]
  );

  /** タスクを更新する */
  const updateTask = useCallback(
    async (id: string, data: UpdateTaskRequest): Promise<TaskResponse> => {
      const updated = await tasksApi.updateTask(id, data);
      setTasks((prev) => prev.map((t) => (t.id === id ? updated : t)));
      return updated;
    },
    []
  );

  /** タスクを削除する */
  const deleteTask = useCallback(
    async (id: string): Promise<void> => {
      await tasksApi.deleteTask(id);
      await fetchTasks();
    },
    [fetchTasks]
  );

  return {
    tasks,
    pagination,
    isLoading,
    error,
    statusFilter,
    setStatusFilter: handleSetStatusFilter,
    page,
    setPage,
    reload: fetchTasks,
    createTask,
    updateTask,
    deleteTask,
  };
}
