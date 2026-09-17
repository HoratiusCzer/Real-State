import type { Metadata } from "next";
import Link from "next/link";
import { requireSession } from "@/lib/auth/session";
import { logoutAction } from "@/lib/auth/actions";
import { notificationsApi } from "@/lib/portal/api";
import { PortalSidebar } from "@/components/portal/sidebar";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = { robots: { index: false } };

/** Protected shell for every /portal/* route (spec §4.2: "all portal routes: authenticated,
 * permission-protected, RLS-protected, noindex"). Authentication is enforced here via
 * requireSession(); permission checks live at the individual action/endpoint level since
 * different portal pages need different permissions; RLS is enforced by REAK.Api regardless of
 * what this layer does (spec §2.5). Deliberately isolated from the (public) route group's
 * marketing chrome (Stage 2 decision). */
export default async function PortalLayout({ children }: { children: React.ReactNode }) {
  const { user, accessToken } = await requireSession();
  const orgName = user.memberships[0]?.memberEntityName;
  const unreadResult = await notificationsApi.unreadCount(accessToken);
  const unreadNotifications = unreadResult.ok ? unreadResult.data.count : 0;

  return (
    <div className="flex min-h-svh flex-col">
      <header className="border-b border-border bg-card">
        <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-3 sm:px-6 lg:px-8">
          <Link href="/portal/dashboard" className="font-heading text-lg font-bold text-foreground">
            REAK <span className="text-muted-foreground font-normal">Member Portal</span>
          </Link>
          <div className="flex items-center gap-4">
            <div className="text-right text-sm leading-tight">
              <p className="font-medium text-foreground">{user.profile.fullName}</p>
              <p className="text-muted-foreground">{orgName ?? user.profile.email}</p>
            </div>
            <form action={logoutAction}>
              <Button type="submit" variant="secondary" size="sm">
                Log out
              </Button>
            </form>
          </div>
        </div>
      </header>

      <div className="mx-auto flex w-full max-w-7xl flex-1 gap-8 px-4 py-8 sm:px-6 lg:px-8">
        <aside className="w-56 shrink-0">
          <PortalSidebar unreadNotifications={unreadNotifications} />
        </aside>
        <main className="min-w-0 flex-1">{children}</main>
      </div>
    </div>
  );
}
