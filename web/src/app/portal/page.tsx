import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { getCurrentUser } from "@/lib/auth/session";
import { logoutAction } from "@/lib/auth/actions";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/ui/empty-state";
import { LayoutDashboard } from "lucide-react";

export const metadata: Metadata = { title: "Member Portal", robots: { index: false } };

/**
 * Deliberately not the real Member Portal (that's Stage 5) — this exists only to prove Stage
 * 4's auth round-trip end-to-end (login sets cookies -> this page reads them -> REAK.Api
 * verifies the JWT -> RLS-scoped data would apply from here on). No portal navigation, tables,
 * or feature UI belongs here yet; a full sidebar/dashboard would be exactly the kind of
 * ahead-of-spec build spec §22/§35 warns against. /portal and /admin intentionally sit outside
 * the (public) route group so they don't inherit the marketing site chrome (Stage 2 decision).
 */
export default async function PortalPlaceholderPage() {
  const user = await getCurrentUser();
  if (!user) {
    redirect("/login");
  }

  return (
    <main className="mx-auto flex min-h-svh max-w-2xl flex-col justify-center px-4 py-16 sm:px-6 lg:px-8">
      <EmptyState
        icon={LayoutDashboard}
        title={`Signed in as ${user.profile.fullName}`}
        description={
          user.memberships.length > 0
            ? `Member of ${user.memberships.map((m) => m.memberEntityName).join(", ")}. The Member Portal itself (listings, demands, matches, collaboration) is built in Stage 5.`
            : "No organization membership yet. The Member Portal itself is built in Stage 5."
        }
        action={
          <form action={logoutAction}>
            <Button type="submit" variant="secondary">
              Log out
            </Button>
          </form>
        }
      />
    </main>
  );
}
