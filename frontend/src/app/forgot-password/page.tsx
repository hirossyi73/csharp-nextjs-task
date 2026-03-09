"use client";

import { useState, type FormEvent } from "react";
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
import { forgotPassword } from "@/lib/api/auth";
import { getErrorMessage } from "@/lib/api/client";
import { validateEmail } from "@/lib/utils/validation";

/** パスワードリセット要求ページ */
export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
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

    const emailError = validateEmail(email);
    if (emailError) {
      setFieldErrors({ email: emailError });
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await forgotPassword({ email });
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
            パスワードリセット
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
              <MuiLink component={NextLink} href="/login" variant="body2">
                ログインへ戻る
              </MuiLink>
            </>
          ) : (
            <Box component="form" onSubmit={handleSubmit} noValidate>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                登録済みのメールアドレスを入力してください。パスワードリセット用のリンクを送信します。
              </Typography>

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
                  "リセットメールを送信"
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
