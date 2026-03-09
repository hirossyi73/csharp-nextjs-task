import type { ReactNode } from "react";
import AppHeader from "@/components/layout/AppHeader";

/** タスクページ共通レイアウト。AppHeader を表示する */
export default function TasksLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <AppHeader />
      {children}
    </>
  );
}
