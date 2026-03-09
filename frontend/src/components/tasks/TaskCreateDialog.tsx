"use client";

import { useState, type FormEvent } from "react";
import {
  Alert,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from "@mui/material";
import { getErrorMessage } from "@/lib/api/client";
import { validateTaskTitle } from "@/lib/utils/validation";

/** TaskCreateDialog コンポーネントの Props */
interface TaskCreateDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: { title: string; description?: string }) => Promise<void>;
}

/** タスク作成ダイアログコンポーネント */
export default function TaskCreateDialog({
  open,
  onClose,
  onSubmit,
}: TaskCreateDialogProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState("");
  const [titleError, setTitleError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  /** ダイアログを閉じてフォームをリセットする */
  const handleClose = () => {
    setTitle("");
    setDescription("");
    setError("");
    setTitleError("");
    onClose();
  };

  /** フォーム送信ハンドラー */
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setTitleError("");

    const validationError = validateTaskTitle(title);
    if (validationError) {
      setTitleError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit({
        title: title.trim(),
        description: description.trim() || undefined,
      });
      handleClose();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>タスク作成</DialogTitle>
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
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>キャンセル</Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? <CircularProgress size={24} /> : "作成"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
