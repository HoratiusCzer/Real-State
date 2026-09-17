import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Globe, Mail, MapPin, Phone, Handshake } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { memberEntitiesApi } from "@/lib/portal/api";
import { createCollaborationRequestAction } from "@/lib/collaboration/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";

export const metadata: Metadata = { title: "Member detail" };

export default async function PortalMemberDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { user, accessToken } = await requireSession();
  const result = await memberEntitiesApi.get(accessToken, id);

  if (!result.ok) {
    if (result.status === 404) {
      notFound();
    }
    return <p className="text-sm text-destructive">Couldn&apos;t load this member: {result.error}</p>;
  }

  const entity = result.data;

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">{entity.name}</h1>
        {entity.description ? <p className="mt-2 text-sm text-muted-foreground">{entity.description}</p> : null}
      </div>

      <Card>
        <CardContent className="space-y-3 pt-6">
          {entity.website ? (
            <p className="flex items-center gap-2 text-sm text-foreground">
              <Globe className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
              <a href={entity.website} target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">
                {entity.website}
              </a>
            </p>
          ) : null}
          {entity.email ? (
            <p className="flex items-center gap-2 text-sm text-foreground">
              <Mail className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
              {entity.email}
            </p>
          ) : null}
          {entity.phone ? (
            <p className="flex items-center gap-2 text-sm text-foreground">
              <Phone className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
              {entity.phone}
            </p>
          ) : null}
          {entity.address ? (
            <p className="flex items-center gap-2 text-sm text-foreground">
              <MapPin className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
              {entity.address}
            </p>
          ) : null}
          {!entity.website && !entity.email && !entity.phone && !entity.address ? (
            <p className="text-sm text-muted-foreground">No contact details published yet.</p>
          ) : null}
        </CardContent>
      </Card>

      {!user.memberships.some((m) => m.memberEntityId === id) && user.permissions.includes("collaboration.create") ? (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Handshake className="h-4 w-4" aria-hidden="true" /> Request collaboration
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="mb-3 text-sm text-muted-foreground">
              Not tied to a specific match — a general introduction request that {entity.name} can accept to open a shared workspace (spec §13).
            </p>
            <form action={createCollaborationRequestAction.bind(null, id)} className="space-y-2">
              <Textarea name="message" placeholder="Introduce yourself (optional)…" rows={2} />
              <SubmitButton size="sm">Send request</SubmitButton>
            </form>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}
