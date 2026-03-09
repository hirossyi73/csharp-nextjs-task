import { authClient } from "./client";
import type {
  TaskResponse,
  TaskListResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
  GetTasksParams,
} from "@/types";

/** タスク一覧を取得する */
export async function getTasks(params?: GetTasksParams): Promise<TaskListResponse> {
  const response = await authClient.get<TaskListResponse>("/tasks", { params });
  return response.data;
}

/** タスク詳細を取得する */
export async function getTask(id: string): Promise<TaskResponse> {
  const response = await authClient.get<TaskResponse>(`/tasks/${id}`);
  return response.data;
}

/** タスクを作成する */
export async function createTask(data: CreateTaskRequest): Promise<TaskResponse> {
  const response = await authClient.post<TaskResponse>("/tasks", data);
  return response.data;
}

/** タスクを更新する */
export async function updateTask(id: string, data: UpdateTaskRequest): Promise<TaskResponse> {
  const response = await authClient.put<TaskResponse>(`/tasks/${id}`, data);
  return response.data;
}

/** タスクを削除する */
export async function deleteTask(id: string): Promise<void> {
  await authClient.delete(`/tasks/${id}`);
}
