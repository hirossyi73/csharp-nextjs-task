"use client";

import { useState, type FormEvent } from "react";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Link as MuiLink,
  TextField,
  Typography,
} from "@mui/material";
import NextLink from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";
import { getErrorMessage } from "@/lib/api/client";
import { validateEmail } from "@/lib/utils/validation";

/** ログインフォームコンポーネント */
export default function LoginForm() {
  const router = useRouter();
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  /** フォーム送信ハンドラー */
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setFieldErrors({});

    const errors: Record<string, string> = {};
    const emailError = validateEmail(email);
    if (emailError) errors.email = emailError;
    if (!password) errors.password = "パスワードは必須です";

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    setIsSubmitting(true);
    try {
      await login({ email, password });
      router.push("/tasks");
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} noValidate>
      <Typography variant="h4" component="h1" textAlign="center" gutterBottom>
        ログイン
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <TextField
        label="メールアドレス"
        type="email"
        fullWidth
        margin="normal"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        error={!!fieldErrors.email}
        helperText={fieldErrors.email}
        autoComplete="email"
        autoFocus
      />

      <TextField
        label="パスワード"
        type="password"
        fullWidth
        margin="normal"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        error={!!fieldErrors.password}
        helperText={fieldErrors.password}
        autoComplete="current-password"
      />

      <Button
        type="submit"
        fullWidth
        variant="contained"
        size="large"
        disabled={isSubmitting}
        sx={{ mt: 2, mb: 2 }}
      >
        {isSubmitting ? <CircularProgress size={24} /> : "ログイン"}
      </Button>

      <Box display="flex" justifyContent="space-between">
        <MuiLink component={NextLink} href="/register" variant="body2">
          アカウントを作成
        </MuiLink>
        <MuiLink component={NextLink} href="/forgot-password" variant="body2">
          パスワードを忘れた方
        </MuiLink>
      </Box>
    </Box>
  );
}
