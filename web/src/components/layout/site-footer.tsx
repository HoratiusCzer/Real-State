import Link from "next/link";
import { footerColumns } from "@/lib/site-nav";

export function SiteFooter() {
  return (
    <footer className="border-t border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-2 gap-8 sm:grid-cols-4">
          <div className="col-span-2 sm:col-span-1">
            <span className="font-heading text-lg font-bold text-primary">REAK</span>
            <p className="mt-2 text-sm text-muted-foreground">
              Real estate association platform.
            </p>
          </div>
          {footerColumns.map((column) => (
            <div key={column.heading}>
              <h3 className="font-heading text-sm font-semibold text-foreground">
                {column.heading}
              </h3>
              <ul className="mt-3 flex flex-col gap-2">
                {column.links.map((link) => (
                  <li key={link.href}>
                    <Link
                      href={link.href}
                      className="text-sm text-muted-foreground transition-colors hover:text-primary"
                    >
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
        <div className="mt-10 border-t border-border pt-6 text-xs text-muted-foreground">
          <p>
            &copy; {new Date().getFullYear()} REAK. Committee information, legal name, and
            contact details are not yet configured.
          </p>
        </div>
      </div>
    </footer>
  );
}
