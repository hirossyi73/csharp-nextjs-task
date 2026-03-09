/** タスクのステータス */
export type TaskStatus = "NOT_STARTED" | "IN_PROGRESS" | "COMPLETED";

/** タスクレスポンス */
export interface TaskResponse {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  createdAt: string;
  updatedAt: string;
}

/** タスク一覧レスポンス */
export interface TaskListResponse {
  items: TaskResponse[];
  pagination: PaginationResponse;
}

/** ページネーションレスポンス */
export interface PaginationResponse {
  currentPage: number;
  perPage: number;
  totalCount: number;
  totalPages: number;
}

/** タスク作成リクエスト */
export interface CreateTaskRequest {
  title: string;
  description?: string | null;
}

/** タスク更新リクエスト */
export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  status: TaskStatus;
}

/** タスク一覧取得パラメータ */
export interface GetTasksParams {
  page?: number;
  perPage?: number;
  status?: TaskStatus;
}
