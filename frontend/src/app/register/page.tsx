"use client";

import { Box, Container, Paper } from "@mui/material";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import RegisterForm from "@/components/auth/RegisterForm";
import { useAuth } from "@/hooks/useAuth";

/** ユーザー登録ページ */
export default function RegisterPage() {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      router.replace("/tasks");
    }
  }, [isAuthenticated, isLoading, router]);

  if (isLoading || isAuthenticated) return null;

  return (
    <Container maxWidth="sm">
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <Paper elevation={3} sx={{ p: 4, width: "100%" }}>
          <RegisterForm />
        </Paper>
      </Box>
    </Container>
  );
}
