import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { requireSession } from "@/lib/auth/session";
import { logoutAction } from "@/lib/auth/actions";
import { AdminSidebar } from "@/components/admin/admin-sidebar";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = { robots: { index: false } };

/** Protected shell for every /admin/* route (spec §4.3 Admin Portal — "elevated RBAC"). Every
 * admin API endpoint independently enforces its own permission check (cms.manage, members.*, etc.
 * — see the individual controllers); this layout's isSystemAdmin gate is UX only (spec §2.5:
 * frontend checks are never the real authority), so a MemberAdmin who guesses an /admin URL is
 * redirected before rendering anything, not because the redirect is the security boundary. */
export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const { user } = await requireSession();
  if (!user.isSystemAdmin) {
    redirect("/portal/dashboard");
  }

  return (
    <div className="flex min-h-svh flex-col">
      <header className="border-b border-border bg-card">
        <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-3 sm:px-6 lg:px-8">
          <Link href="/admin" className="font-heading text-lg font-bold text-foreground">
            REAK <span className="text-muted-foreground font-normal">Admin Portal</span>
          </Link>
          <div className="flex items-center gap-4">
            <div className="text-right text-sm leading-tight">
              <p className="font-medium text-foreground">{user.profile.fullName}</p>
              <p className="text-muted-foreground">{user.profile.email}</p>
            </div>
            <Button href="/portal/dashboard" variant="ghost" size="sm">
              Member Portal
            </Button>
            <form action={logoutAction}>
              <Button type="submit" variant="secondary" size="sm">
                Log out
              </Button>
            </form>
          </div>
        </div>
      </header>

      <div className="mx-auto flex w-full max-w-7xl flex-1 gap-8 px-4 py-8 sm:px-6 lg:px-8">
        <aside className="w-60 shrink-0">
          <AdminSidebar />
        </aside>
        <main className="min-w-0 flex-1">{children}</main>
      </div>
    </div>
  );
}
