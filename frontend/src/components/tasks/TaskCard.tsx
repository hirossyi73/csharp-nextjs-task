"use client";

import {
  Card,
  CardActions,
  CardContent,
  Chip,
  IconButton,
  Typography,
} from "@mui/material";
import { DeleteOutline, EditOutlined } from "@mui/icons-material";
import type { TaskResponse, TaskStatus } from "@/types";

/** ステータスの表示ラベルと色を定義する */
const STATUS_CONFIG: Record<
  TaskStatus,
  { label: string; color: "default" | "primary" | "success" }
> = {
  NOT_STARTED: { label: "未着手", color: "default" },
  IN_PROGRESS: { label: "進行中", color: "primary" },
  COMPLETED: { label: "完了", color: "success" },
};

/** TaskCard コンポーネントの Props */
interface TaskCardProps {
  task: TaskResponse;
  onEdit: (task: TaskResponse) => void;
  onDelete: (task: TaskResponse) => void;
}

/** タスクカードコンポーネント */
export default function TaskCard({ task, onEdit, onDelete }: TaskCardProps) {
  const statusConfig = STATUS_CONFIG[task.status];

  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h6" component="div" gutterBottom>
          {task.title}
        </Typography>
        {task.description && (
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            {task.description}
          </Typography>
        )}
        <Chip
          label={statusConfig.label}
          color={statusConfig.color}
          size="small"
        />
      </CardContent>
      <CardActions sx={{ justifyContent: "flex-end" }}>
        <IconButton size="small" onClick={() => onEdit(task)} aria-label="編集">
          <EditOutlined />
        </IconButton>
        <IconButton
          size="small"
          onClick={() => onDelete(task)}
          aria-label="削除"
          color="error"
        >
          <DeleteOutline />
        </IconButton>
      </CardActions>
    </Card>
  );
}
