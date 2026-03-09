"use client";

import { Box, Pagination, Stack, Typography } from "@mui/material";
import TaskCard from "./TaskCard";
import type { PaginationResponse, TaskResponse } from "@/types";

/** TaskList コンポーネントの Props */
interface TaskListProps {
  tasks: TaskResponse[];
  pagination: PaginationResponse | null;
  onEdit: (task: TaskResponse) => void;
  onDelete: (task: TaskResponse) => void;
  onPageChange: (page: number) => void;
}

/** タスク一覧コンポーネント */
export default function TaskList({
  tasks,
  pagination,
  onEdit,
  onDelete,
  onPageChange,
}: TaskListProps) {
  if (tasks.length === 0) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="body1" color="text.secondary">
          タスクがありません
        </Typography>
      </Box>
    );
  }

  return (
    <Stack spacing={2}>
      {tasks.map((task) => (
        <TaskCard key={task.id} task={task} onEdit={onEdit} onDelete={onDelete} />
      ))}
      {pagination && pagination.totalPages > 1 && (
        <Box display="flex" justifyContent="center" pt={2}>
          <Pagination
            count={pagination.totalPages}
            page={pagination.currentPage}
            onChange={(_, page) => onPageChange(page)}
          />
        </Box>
      )}
    </Stack>
  );
}
