import type { Metadata } from "next";
import ThemeRegistry from "@/components/common/ThemeRegistry";

export const metadata: Metadata = {
  title: "TaskFlow",
  description: "タスク管理アプリケーション",
};

/** アプリケーションのルートレイアウト。MUI テーマを全ページに適用する */
export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="ja">
      <body>
        <ThemeRegistry>{children}</ThemeRegistry>
      </body>
    </html>
  );
}
