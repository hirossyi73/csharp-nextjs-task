"use client";

import { useState } from "react";
import {
  Alert,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
} from "@mui/material";
import { getErrorMessage } from "@/lib/api/client";
import type { TaskResponse } from "@/types";

/** TaskDeleteConfirmDialog コンポーネントの Props */
interface TaskDeleteConfirmDialogProps {
  open: boolean;
  task: TaskResponse | null;
  onClose: () => void;
  onConfirm: (id: string) => Promise<void>;
}

/** タスク削除確認ダイアログコンポーネント */
export default function TaskDeleteConfirmDialog({
  open,
  task,
  onClose,
  onConfirm,
}: TaskDeleteConfirmDialogProps) {
  const [error, setError] = useState("");
  const [isDeleting, setIsDeleting] = useState(false);

  /** 削除実行ハンドラー */
  const handleDelete = async () => {
    if (!task) return;

    setError("");
    setIsDeleting(true);
    try {
      await onConfirm(task.id);
      onClose();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>タスクの削除</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          「{task?.title}」を削除してもよろしいですか？この操作は取り消せません。
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>キャンセル</Button>
        <Button
          onClick={handleDelete}
          color="error"
          variant="contained"
          disabled={isDeleting}
        >
          {isDeleting ? <CircularProgress size={24} /> : "削除"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
