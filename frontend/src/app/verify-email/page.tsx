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
import { verifyEmail } from "@/lib/api/auth";
import { getErrorMessage } from "@/lib/api/client";
import {
  validatePassword,
  validatePasswordConfirmation,
} from "@/lib/utils/validation";

/** メール確認・パスワード設定ページ */
export default function VerifyEmailPage() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [password, setPassword] = useState("");
  const [passwordConfirmation, setPasswordConfirmation] = useState("");
  const [status, setStatus] = useState<"form" | "submitting" | "success" | "error">(
    token ? "form" : "error"
  );
  const [message, setMessage] = useState(
    token ? "" : "トークンが指定されていません"
  );
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  /** フォーム送信ハンドラー */
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFieldErrors({});
    setMessage("");

    const errors: Record<string, string> = {};
    const passwordError = validatePassword(password);
    if (passwordError) errors.password = passwordError;
    const confirmError = validatePasswordConfirmation(password, passwordConfirmation);
    if (confirmError) errors.passwordConfirmation = confirmError;

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    setStatus("submitting");
    try {
      const response = await verifyEmail({
        token: token!,
        password,
        passwordConfirmation,
      });
      setStatus("success");
      setMessage(response.message);
    } catch (err) {
      setStatus("error");
      setMessage(getErrorMessage(err));
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
            アカウント登録
          </Typography>

          {status === "form" && (
            <Box component="form" onSubmit={handleSubmit} noValidate>
              <Typography variant="body1" sx={{ mb: 2 }}>
                パスワードを設定して登録を完了してください。
              </Typography>

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
                autoFocus
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
                sx={{ mt: 2, mb: 2 }}
              >
                登録を完了する
              </Button>
            </Box>
          )}

          {status === "submitting" && (
            <Box textAlign="center" sx={{ my: 4 }}>
              <CircularProgress />
            </Box>
          )}

          {status === "success" && (
            <>
              <Alert severity="success" sx={{ mb: 2 }}>
                {message}
              </Alert>
              <Box textAlign="center">
                <MuiLink component={NextLink} href="/login" variant="body1">
                  ログインへ
                </MuiLink>
              </Box>
            </>
          )}

          {status === "error" && (
            <>
              <Alert severity="error" sx={{ mb: 2 }}>
                {message}
              </Alert>
              <Box textAlign="center">
                <MuiLink component={NextLink} href="/login" variant="body1">
                  ログインへ戻る
                </MuiLink>
              </Box>
            </>
          )}
        </Paper>
      </Box>
    </Container>
  );
}
