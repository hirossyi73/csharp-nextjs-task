"use client";

import { AppBar, Box, Button, Toolbar, Typography } from "@mui/material";
import { LogoutOutlined } from "@mui/icons-material";
import { useRouter } from "next/navigation";
import { useAuth } from "@/hooks/useAuth";

/** アプリケーションヘッダーコンポーネント */
export default function AppHeader() {
  const { isAuthenticated, logout } = useAuth();
  const router = useRouter();

  /** ログアウトハンドラー */
  const handleLogout = async () => {
    await logout();
    router.push("/login");
  };

  if (!isAuthenticated) return null;

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          TaskFlow
        </Typography>
        <Box>
          <Button
            color="inherit"
            onClick={handleLogout}
            startIcon={<LogoutOutlined />}
          >
            ログアウト
          </Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
}
