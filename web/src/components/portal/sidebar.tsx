"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { portalNavItems } from "@/lib/portal-nav";

export function PortalSidebar({ unreadNotifications = 0 }: { unreadNotifications?: number }) {
  const pathname = usePathname();

  return (
    <nav aria-label="Member Portal" className="space-y-1">
      {portalNavItems.map((item) => {
        const active = pathname === item.href || pathname.startsWith(`${item.href}/`);
        const Icon = item.icon;
        return (
          <Link
            key={item.href}
            href={item.href}
            aria-current={active ? "page" : undefined}
            className={cn(
              "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
              active
                ? "bg-accent text-on-accent"
                : "text-foreground hover:bg-muted"
            )}
          >
            <Icon className="h-4 w-4 shrink-0" aria-hidden="true" />
            <span className="flex-1">{item.label}</span>
            {item.href === "/portal/notifications" && unreadNotifications > 0 ? (
              <span
                className={cn(
                  "inline-flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-xs font-semibold",
                  active ? "bg-on-accent/20 text-on-accent" : "bg-accent text-on-accent"
                )}
              >
                {unreadNotifications > 99 ? "99+" : unreadNotifications}
              </span>
            ) : null}
          </Link>
        );
      })}
    </nav>
  );
}
