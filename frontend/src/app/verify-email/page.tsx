"use client";

import { useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import {
  Alert,
  Box,
  CircularProgress,
  Container,
  Link as MuiLink,
  Paper,
  Typography,
} from "@mui/material";
import NextLink from "next/link";
import { verifyEmail } from "@/lib/api/auth";
import { getErrorMessage } from "@/lib/api/client";

/** メール確認ページ */
export default function VerifyEmailPage() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [status, setStatus] = useState<"loading" | "success" | "error">(
    "loading"
  );
  const [message, setMessage] = useState("");

  useEffect(() => {
    if (!token) {
      setStatus("error");
      setMessage("トークンが指定されていません");
      return;
    }

    const verify = async () => {
      try {
        const response = await verifyEmail({ token });
        setStatus("success");
        setMessage(response.message);
      } catch (err) {
        setStatus("error");
        setMessage(getErrorMessage(err));
      }
    };
    verify();
  }, [token]);

  return (
    <Container maxWidth="sm">
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <Paper elevation={3} sx={{ p: 4, width: "100%", textAlign: "center" }}>
          <Typography
            variant="h4"
            component="h1"
            textAlign="center"
            gutterBottom
          >
            メール確認
          </Typography>

          {status === "loading" && <CircularProgress sx={{ my: 4 }} />}

          {status === "success" && (
            <>
              <Alert severity="success" sx={{ mb: 2 }}>
                {message}
              </Alert>
              <MuiLink component={NextLink} href="/login" variant="body1">
                ログインへ
              </MuiLink>
            </>
          )}

          {status === "error" && (
            <>
              <Alert severity="error" sx={{ mb: 2 }}>
                {message}
              </Alert>
              <MuiLink component={NextLink} href="/login" variant="body1">
                ログインへ戻る
              </MuiLink>
            </>
          )}
        </Paper>
      </Box>
    </Container>
  );
}
