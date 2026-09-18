"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { Menu, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { adminNavSections } from "@/lib/admin-nav";

// Same fix as PortalSidebar (see its header comment) — a 22-item, 5-section nav with no mobile
// behavior at all was an even more severe version of the same "shrunken desktop" problem.
export function AdminSidebar() {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  // Reset the mobile menu on navigation without an effect — see PortalSidebar's header comment
  // for why (the react-hooks/set-state-in-effect rule).
  const [prevPathname, setPrevPathname] = useState(pathname);
  if (pathname !== prevPathname) {
    setPrevPathname(pathname);
    setOpen(false);
  }

  return (
    <>
      <button
        type="button"
        className="mb-3 flex w-full items-center gap-2 rounded-md border border-border bg-card px-3 py-2 text-sm font-medium text-foreground md:hidden"
        aria-expanded={open}
        aria-controls="admin-nav"
        onClick={() => setOpen((v) => !v)}
      >
        {open ? <X className="h-4 w-4" aria-hidden="true" /> : <Menu className="h-4 w-4" aria-hidden="true" />}
        Menu
      </button>
      <nav id="admin-nav" aria-label="Admin Portal" className={cn("space-y-5", open ? "block" : "hidden", "md:block")}>
        {adminNavSections.map((section) => (
          <div key={section.heading}>
            <p className="px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">{section.heading}</p>
            <div className="mt-1 space-y-1">
              {section.items.map((item) => {
                const active = pathname === item.href || pathname.startsWith(`${item.href}/`);
                const Icon = item.icon;
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    aria-current={active ? "page" : undefined}
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                      active ? "bg-accent text-on-accent" : "text-foreground hover:bg-muted"
                    )}
                  >
                    <Icon className="h-4 w-4 shrink-0" aria-hidden="true" />
                    {item.label}
                  </Link>
                );
              })}
            </div>
          </div>
        ))}
      </nav>
    </>
  );
}
