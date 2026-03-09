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
import { useAuth } from "@/hooks/useAuth";
import { getErrorMessage } from "@/lib/api/client";
import {
  validateEmail,
  validatePassword,
  validatePasswordConfirmation,
} from "@/lib/utils/validation";

/** ユーザー登録フォームコンポーネント */
export default function RegisterForm() {
  const { register } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [passwordConfirmation, setPasswordConfirmation] = useState("");
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  /** フォーム送信ハンドラー */
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");
    setFieldErrors({});

    const errors: Record<string, string> = {};
    const emailError = validateEmail(email);
    if (emailError) errors.email = emailError;
    const passwordError = validatePassword(password);
    if (passwordError) errors.password = passwordError;
    const confirmError = validatePasswordConfirmation(
      password,
      passwordConfirmation
    );
    if (confirmError) errors.passwordConfirmation = confirmError;

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    setIsSubmitting(true);
    try {
      const message = await register({ email, password, passwordConfirmation });
      setSuccessMessage(message);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  if (successMessage) {
    return (
      <Box>
        <Typography variant="h4" component="h1" textAlign="center" gutterBottom>
          登録完了
        </Typography>
        <Alert severity="success" sx={{ mb: 2 }}>
          {successMessage}
        </Alert>
        <MuiLink component={NextLink} href="/login" variant="body2">
          ログインへ戻る
        </MuiLink>
      </Box>
    );
  }

  return (
    <Box component="form" onSubmit={handleSubmit} noValidate>
      <Typography variant="h4" component="h1" textAlign="center" gutterBottom>
        アカウント作成
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
        helperText={
          fieldErrors.password || "8文字以上、大文字・小文字・数字を含む"
        }
        autoComplete="new-password"
      />

      <TextField
        label="パスワード確認"
        type="password"
        fullWidth
        margin="normal"
        value={passwordConfirmation}
        onChange={(e) => setPasswordConfirmation(e.target.value)}
        error={!!fieldErrors.passwordConfirmation}
        helperText={fieldErrors.passwordConfirmation}
        autoComplete="new-password"
      />

      <Button
        type="submit"
        fullWidth
        variant="contained"
        size="large"
        disabled={isSubmitting}
        sx={{ mt: 2, mb: 2 }}
      >
        {isSubmitting ? <CircularProgress size={24} /> : "登録"}
      </Button>

      <MuiLink component={NextLink} href="/login" variant="body2">
        すでにアカウントをお持ちの方
      </MuiLink>
    </Box>
  );
}
