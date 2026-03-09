"use client";

import { ToggleButton, ToggleButtonGroup } from "@mui/material";
import type { TaskStatus } from "@/types";

/** TaskStatusFilter コンポーネントの Props */
interface TaskStatusFilterProps {
  value: TaskStatus | undefined;
  onChange: (status: TaskStatus | undefined) => void;
}

/** タスクステータスフィルターコンポーネント */
export default function TaskStatusFilter({
  value,
  onChange,
}: TaskStatusFilterProps) {
  /** フィルター変更ハンドラー */
  const handleChange = (
    _: React.MouseEvent<HTMLElement>,
    newValue: TaskStatus | null
  ) => {
    onChange(newValue ?? undefined);
  };

  return (
    <ToggleButtonGroup
      value={value ?? null}
      exclusive
      onChange={handleChange}
      size="small"
    >
      <ToggleButton value="NOT_STARTED">未着手</ToggleButton>
      <ToggleButton value="IN_PROGRESS">進行中</ToggleButton>
      <ToggleButton value="COMPLETED">完了</ToggleButton>
    </ToggleButtonGroup>
  );
}
