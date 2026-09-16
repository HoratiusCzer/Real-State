"use client";

import { SubmitButton } from "@/components/auth/submit-button";
import type { ButtonProps } from "@/components/ui/button";

/** Wraps a Server-Action form's submit in a native confirm() for destructive actions (delete,
 * archive) — the only bit of client JS these detail-page actions need. */
export function ConfirmSubmitButton({
  confirmMessage,
  children,
  variant,
}: {
  confirmMessage: string;
  children: React.ReactNode;
  variant?: ButtonProps["variant"];
}) {
  return (
    <SubmitButton
      variant={variant}
      onClick={(e) => {
        if (!window.confirm(confirmMessage)) {
          e.preventDefault();
        }
      }}
    >
      {children}
    </SubmitButton>
  );
}
