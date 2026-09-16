import * as React from "react";
import { AlertTriangle, CheckCircle2, Info, XCircle } from "lucide-react";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const alertVariants = cva("flex gap-3 rounded-md border p-4 text-sm", {
  variants: {
    variant: {
      info: "border-border bg-muted text-foreground",
      success: "border-success/30 bg-success/10 text-success",
      warning: "border-amber-300 bg-amber-50 text-amber-800",
      destructive: "border-destructive/30 bg-destructive/10 text-destructive",
    },
  },
  defaultVariants: { variant: "info" },
});

const icons = {
  info: Info,
  success: CheckCircle2,
  warning: AlertTriangle,
  destructive: XCircle,
} as const;

export function Alert({
  className,
  variant = "info",
  title,
  children,
  ...props
}: React.HTMLAttributes<HTMLDivElement> &
  VariantProps<typeof alertVariants> & { title?: string }) {
  const Icon = icons[variant ?? "info"];
  return (
    <div role="alert" className={cn(alertVariants({ variant }), className)} {...props}>
      <Icon className="h-5 w-5 shrink-0" aria-hidden="true" />
      <div>
        {title ? <p className="font-semibold">{title}</p> : null}
        <div className="text-sm/relaxed">{children}</div>
      </div>
    </div>
  );
}
