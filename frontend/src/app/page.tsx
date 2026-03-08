import { redirect } from "next/navigation";

/** ルートページ。ログイン画面にリダイレクトする */
export default function Home() {
  redirect("/login");
}
