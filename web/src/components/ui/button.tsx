import * as React from "react";
import Link from "next/link";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-semibold transition-colors duration-200 cursor-pointer disabled:pointer-events-none disabled:opacity-50 disabled:cursor-not-allowed focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring",
  {
    variants: {
      variant: {
        primary: "bg-accent text-on-accent hover:opacity-90",
        secondary:
          "border border-primary text-primary bg-transparent hover:bg-muted",
        ghost: "text-primary hover:bg-muted",
        destructive: "bg-destructive text-on-destructive hover:opacity-90",
      },
      size: {
        sm: "h-9 px-3 text-xs",
        md: "h-11 px-5",
        lg: "h-12 px-6 text-base",
      },
    },
    defaultVariants: {
      variant: "primary",
      size: "md",
    },
  }
);

type ButtonBaseProps = VariantProps<typeof buttonVariants> & {
  className?: string;
  children: React.ReactNode;
};

type ButtonAsButton = ButtonBaseProps &
  React.ButtonHTMLAttributes<HTMLButtonElement> & { href?: undefined };

type ButtonAsLink = ButtonBaseProps &
  React.AnchorHTMLAttributes<HTMLAnchorElement> & { href: string };

export type ButtonProps = ButtonAsButton | ButtonAsLink;

export function Button({ className, variant, size, ...props }: ButtonProps) {
  const classes = cn(buttonVariants({ variant, size }), className);

  if ("href" in props && props.href) {
    const { href, ...rest } = props;
    return (
      <Link href={href} className={classes} {...rest}>
        {props.children}
      </Link>
    );
  }

  const { ...rest } = props as ButtonAsButton;
  return (
    <button className={classes} {...rest}>
      {props.children}
    </button>
  );
}
