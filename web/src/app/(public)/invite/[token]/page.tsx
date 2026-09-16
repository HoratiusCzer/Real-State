import type { Metadata } from "next";
import { invitationsApi } from "@/lib/auth/api";
import { AcceptInvitationForm } from "@/components/auth/accept-invitation-form";
import { Alert } from "@/components/ui/alert";

export const metadata: Metadata = { title: "Accept invitation", robots: { index: false } };

/**
 * Flow A step 4 (spec §2.4): the invitee lands here from the emailed invitation link. Not one
 * of the spec §4.1's original 20 public routes — added because Flow A can't complete without it
 * once Stage 4 makes invitations real; kept under the (public) route group like login/forgot
 * password since it's unauthenticated by definition.
 */
export default async function AcceptInvitationPage({ params }: { params: Promise<{ token: string }> }) {
  const { token } = await params;
  const result = await invitationsApi.lookup(token);

  return (
    <section className="mx-auto max-w-md px-4 py-16 sm:px-6 lg:px-8">
      <h1 className="font-heading text-3xl font-bold text-foreground">Accept invitation</h1>

      {!result.ok || !result.data.found ? (
        <div className="mt-8">
          <Alert variant="destructive">This invitation link is invalid.</Alert>
        </div>
      ) : result.data.isExpiredOrUsed ? (
        <div className="mt-8">
          <Alert variant="destructive">
            This invitation has expired or has already been used. Contact your REAK administrator for a new
            one.
          </Alert>
        </div>
      ) : (
        <>
          <p className="mt-2 text-sm text-muted-foreground">
            {result.data.email} is invited to join
            {result.data.memberEntityName ? ` ${result.data.memberEntityName}` : ""} on REAK as{" "}
            {result.data.roleName}.
          </p>
          <div className="mt-8">
            <AcceptInvitationForm token={token} isNewAccount={result.data.isNewAccount} />
          </div>
        </>
      )}
    </section>
  );
}
