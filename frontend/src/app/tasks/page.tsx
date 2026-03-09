"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Alert,
  Box,
  CircularProgress,
  Container,
  Fab,
  Typography,
} from "@mui/material";
import { Add } from "@mui/icons-material";
import { useAuth } from "@/hooks/useAuth";
import { useTasks } from "@/hooks/useTasks";
import TaskList from "@/components/tasks/TaskList";
import TaskStatusFilter from "@/components/tasks/TaskStatusFilter";
import TaskCreateDialog from "@/components/tasks/TaskCreateDialog";
import TaskEditDialog from "@/components/tasks/TaskEditDialog";
import TaskDeleteConfirmDialog from "@/components/tasks/TaskDeleteConfirmDialog";
import type { TaskResponse, UpdateTaskRequest } from "@/types";

/** タスク一覧ページ */
export default function TasksPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const {
    tasks,
    pagination,
    isLoading,
    error,
    statusFilter,
    setStatusFilter,
    page,
    setPage,
    createTask,
    updateTask,
    deleteTask,
  } = useTasks();

  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [editTask, setEditTask] = useState<TaskResponse | null>(null);
  const [deleteTaskTarget, setDeleteTaskTarget] =
    useState<TaskResponse | null>(null);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.replace("/login");
    }
  }, [authLoading, isAuthenticated, router]);

  /** タスク作成ハンドラー */
  const handleCreate = useCallback(
    async (data: { title: string; description?: string }) => {
      await createTask(data);
    },
    [createTask]
  );

  /** タスク更新ハンドラー */
  const handleUpdate = useCallback(
    async (id: string, data: UpdateTaskRequest) => {
      await updateTask(id, data);
    },
    [updateTask]
  );

  /** タスク削除ハンドラー */
  const handleDelete = useCallback(
    async (id: string) => {
      await deleteTask(id);
    },
    [deleteTask]
  );

  if (authLoading || !isAuthenticated) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
      >
        <Typography variant="h4" component="h1">
          タスク一覧
        </Typography>
        <TaskStatusFilter value={statusFilter} onChange={setStatusFilter} />
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {isLoading ? (
        <Box display="flex" justifyContent="center" py={4}>
          <CircularProgress />
        </Box>
      ) : (
        <TaskList
          tasks={tasks}
          pagination={pagination}
          onEdit={setEditTask}
          onDelete={setDeleteTaskTarget}
          onPageChange={setPage}
        />
      )}

      {pagination && (
        <Typography
          variant="body2"
          color="text.secondary"
          textAlign="center"
          mt={2}
        >
          全 {pagination.totalCount} 件（{page} / {pagination.totalPages}{" "}
          ページ）
        </Typography>
      )}

      <Fab
        color="primary"
        aria-label="タスク作成"
        onClick={() => setCreateDialogOpen(true)}
        sx={{ position: "fixed", bottom: 24, right: 24 }}
      >
        <Add />
      </Fab>

      <TaskCreateDialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        onSubmit={handleCreate}
      />

      <TaskEditDialog
        open={!!editTask}
        task={editTask}
        onClose={() => setEditTask(null)}
        onSubmit={handleUpdate}
      />

      <TaskDeleteConfirmDialog
        open={!!deleteTaskTarget}
        task={deleteTaskTarget}
        onClose={() => setDeleteTaskTarget(null)}
        onConfirm={handleDelete}
      />
    </Container>
  );
}
