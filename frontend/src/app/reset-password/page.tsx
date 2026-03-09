"use client";

import { useState, type FormEvent } from "react";
import { useSearchParams } from "next/navigation";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Container,
  Link as MuiLink,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import NextLink from "next/link";
import { resetPassword } from "@/lib/api/auth";
import { getErrorMessage } from "@/lib/api/client";
import {
  validatePassword,
  validatePasswordConfirmation,
} from "@/lib/utils/validation";

/** パスワード再設定ページ */
export default function ResetPasswordPage() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [newPassword, setNewPassword] = useState("");
  const [newPasswordConfirmation, setNewPasswordConfirmation] = useState("");
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

    if (!token) {
      setError("トークンが指定されていません");
      return;
    }

    const errors: Record<string, string> = {};
    const passwordError = validatePassword(newPassword);
    if (passwordError) errors.newPassword = passwordError;
    const confirmError = validatePasswordConfirmation(
      newPassword,
      newPasswordConfirmation
    );
    if (confirmError) errors.newPasswordConfirmation = confirmError;

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await resetPassword({
        token,
        newPassword,
        newPasswordConfirmation,
      });
      setSuccessMessage(response.message);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <Paper elevation={3} sx={{ p: 4, width: "100%" }}>
          <Typography
            variant="h4"
            component="h1"
            textAlign="center"
            gutterBottom
          >
            パスワード再設定
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          {successMessage ? (
            <>
              <Alert severity="success" sx={{ mb: 2 }}>
                {successMessage}
              </Alert>
              <MuiLink component={NextLink} href="/login" variant="body1">
                ログインへ
              </MuiLink>
            </>
          ) : (
            <Box component="form" onSubmit={handleSubmit} noValidate>
              <TextField
                label="新しいパスワード"
                type="password"
                fullWidth
                margin="normal"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                error={!!fieldErrors.newPassword}
                helperText={
                  fieldErrors.newPassword ||
                  "8文字以上、大文字・小文字・数字を含む"
                }
                autoComplete="new-password"
                autoFocus
              />

              <TextField
                label="新しいパスワード確認"
                type="password"
                fullWidth
                margin="normal"
                value={newPasswordConfirmation}
                onChange={(e) => setNewPasswordConfirmation(e.target.value)}
                error={!!fieldErrors.newPasswordConfirmation}
                helperText={fieldErrors.newPasswordConfirmation}
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
                {isSubmitting ? (
                  <CircularProgress size={24} />
                ) : (
                  "パスワードを再設定"
                )}
              </Button>

              <MuiLink component={NextLink} href="/login" variant="body2">
                ログインへ戻る
              </MuiLink>
            </Box>
          )}
        </Paper>
      </Box>
    </Container>
  );
}
