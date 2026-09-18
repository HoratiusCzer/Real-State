"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { Menu, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { portalNavItems } from "@/lib/portal-nav";

// Was a plain always-visible vertical list with no mobile behavior at all — on a narrow viewport
// it squeezed page content into an unusably thin column alongside a fixed-width sidebar, exactly
// the "shrunken desktop" anti-pattern spec §23 warns against (just applied to navigation instead
// of a table). Below md, this collapses into the same disclosure-button pattern SiteHeader's
// mobile nav already uses (aria-expanded/aria-controls, closes automatically on navigation).
export function PortalSidebar({ unreadNotifications = 0 }: { unreadNotifications?: number }) {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  // Reset the mobile menu on navigation without an effect (React's own recommended pattern for
  // "adjust state when a prop/derived value changes" — see the react-hooks/set-state-in-effect
  // rule): compare against the last-rendered pathname during render itself.
  const [prevPathname, setPrevPathname] = useState(pathname);
  if (pathname !== prevPathname) {
    setPrevPathname(pathname);
    setOpen(false);
  }

  return (
    <>
      <button
        type="button"
        className="mb-3 flex w-full items-center justify-between gap-2 rounded-md border border-border bg-card px-3 py-2 text-sm font-medium text-foreground md:hidden"
        aria-expanded={open}
        aria-controls="portal-nav"
        onClick={() => setOpen((v) => !v)}
      >
        <span className="flex items-center gap-2">
          {open ? <X className="h-4 w-4" aria-hidden="true" /> : <Menu className="h-4 w-4" aria-hidden="true" />}
          Menu
        </span>
        {unreadNotifications > 0 ? (
          <span className="inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-accent px-1.5 text-xs font-semibold text-on-accent">
            {unreadNotifications > 99 ? "99+" : unreadNotifications}
          </span>
        ) : null}
      </button>
      <nav id="portal-nav" aria-label="Member Portal" className={cn("space-y-1", open ? "block" : "hidden", "md:block")}>
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
    </>
  );
}
