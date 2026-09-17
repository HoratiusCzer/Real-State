"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { adminNavSections } from "@/lib/admin-nav";

export function AdminSidebar() {
  const pathname = usePathname();

  return (
    <nav aria-label="Admin Portal" className="space-y-5">
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
  );
}
