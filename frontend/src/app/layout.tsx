import type { Metadata } from "next";
import ThemeRegistry from "@/components/common/ThemeRegistry";
import { AuthProvider } from "@/contexts/AuthContext";

export const metadata: Metadata = {
  title: "TaskFlow",
  description: "タスク管理アプリケーション",
};

/** アプリケーションのルートレイアウト。MUI テーマと認証プロバイダーを全ページに適用する */
export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="ja">
      <body>
        <ThemeRegistry>
          <AuthProvider>{children}</AuthProvider>
        </ThemeRegistry>
      </body>
    </html>
  );
}
