"use client";

import { useEffect, useState, type FormEvent } from "react";
import {
  Alert,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from "@mui/material";
import { getErrorMessage } from "@/lib/api/client";
import { validateTaskTitle } from "@/lib/utils/validation";
import type { TaskResponse, TaskStatus, UpdateTaskRequest } from "@/types";

/** TaskEditDialog コンポーネントの Props */
interface TaskEditDialogProps {
  open: boolean;
  task: TaskResponse | null;
  onClose: () => void;
  onSubmit: (id: string, data: UpdateTaskRequest) => Promise<void>;
}

/** タスク編集ダイアログコンポーネント */
export default function TaskEditDialog({
  open,
  task,
  onClose,
  onSubmit,
}: TaskEditDialogProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<TaskStatus>("NOT_STARTED");
  const [error, setError] = useState("");
  const [titleError, setTitleError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  // ダイアログが開かれたときにタスクの値を初期値にセット
  useEffect(() => {
    if (task) {
      setTitle(task.title);
      setDescription(task.description ?? "");
      setStatus(task.status);
      setError("");
      setTitleError("");
    }
  }, [task]);

  /** フォーム送信ハンドラー */
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!task) return;

    setError("");
    setTitleError("");

    const validationError = validateTaskTitle(title);
    if (validationError) {
      setTitleError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit(task.id, {
        title: title.trim(),
        description: description.trim() || null,
        status,
      });
      onClose();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>タスク編集</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            label="タイトル"
            fullWidth
            margin="normal"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            error={!!titleError}
            helperText={titleError}
            autoFocus
          />
          <TextField
            label="説明（任意）"
            fullWidth
            margin="normal"
            multiline
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <FormControl fullWidth margin="normal">
            <InputLabel>ステータス</InputLabel>
            <Select
              value={status}
              label="ステータス"
              onChange={(e) => setStatus(e.target.value as TaskStatus)}
            >
              <MenuItem value="NOT_STARTED">未着手</MenuItem>
              <MenuItem value="IN_PROGRESS">進行中</MenuItem>
              <MenuItem value="COMPLETED">完了</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>キャンセル</Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? <CircularProgress size={24} /> : "更新"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
