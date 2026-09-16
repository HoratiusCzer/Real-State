import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Trash2 } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { demandsApi } from "@/lib/demands/api";
import { archiveDemandAction, deleteDemandAction, fulfillDemandAction, publishDemandAction } from "@/lib/demands/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { SubmitButton } from "@/components/auth/submit-button";
import { ConfirmSubmitButton } from "@/components/listings/confirm-submit-button";
import { demandStatusBadgeVariant } from "@/lib/demands/status-badge";

export const metadata: Metadata = { title: "Requirement detail" };

export default async function DemandDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();

  const result = await demandsApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this requirement: {result.error}</p>;
  }
  const demand = result.data;

  const contactResult = demand.isOwner ? await demandsApi.getContact(accessToken, id) : null;

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <div className="flex items-center gap-2">
          <h1 className="font-heading text-2xl font-bold text-foreground">{demand.title}</h1>
          <Badge variant={demandStatusBadgeVariant(demand.status)}>{demand.status}</Badge>
        </div>
        <p className="mt-1 text-sm text-muted-foreground">{demand.referenceCode} · {demand.memberEntityName}</p>
      </div>

      <Card>
        <CardHeader><CardTitle>Requirement</CardTitle></CardHeader>
        <CardContent className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-3">
          <Detail label="Purpose" value={demand.purposeName} />
          {demand.minBudget || demand.maxBudget ? (
            <Detail label="Budget" value={`${demand.currencyCode ?? ""} ${demand.minBudget?.toLocaleString() ?? "?"}–${demand.maxBudget?.toLocaleString() ?? "?"}`} />
          ) : null}
          {demand.minArea || demand.maxArea ? (
            <Detail label="Area" value={`${demand.minArea ?? "?"}–${demand.maxArea ?? "?"} ${demand.areaUnitName ?? ""}`} />
          ) : null}
          {demand.minBedrooms != null ? <Detail label="Min bedrooms" value={String(demand.minBedrooms)} /> : null}
          {demand.minBathrooms != null ? <Detail label="Min bathrooms" value={String(demand.minBathrooms)} /> : null}
        </CardContent>
      </Card>

      {demand.propertyTypeNames.length > 0 ? (
        <Card>
          <CardHeader><CardTitle>Property types</CardTitle></CardHeader>
          <CardContent className="flex flex-wrap gap-2">{demand.propertyTypeNames.map((t) => <Badge key={t}>{t}</Badge>)}</CardContent>
        </Card>
      ) : null}

      {demand.locations.length > 0 ? (
        <Card>
          <CardHeader><CardTitle>Acceptable locations</CardTitle></CardHeader>
          <CardContent className="space-y-1 text-sm text-foreground">
            {demand.locations.map((l) => (
              <p key={l.id}>
                {[l.localityName, l.wardNumber ? `Ward ${l.wardNumber}` : null, l.municipalityName, l.districtName, l.provinceName].filter(Boolean).join(", ")}
              </p>
            ))}
          </CardContent>
        </Card>
      ) : null}

      {demand.description ? (
        <Card><CardHeader><CardTitle>Description</CardTitle></CardHeader><CardContent><p className="whitespace-pre-wrap text-sm text-foreground">{demand.description}</p></CardContent></Card>
      ) : null}

      {demand.amenityNames.length > 0 ? (
        <Card><CardHeader><CardTitle>Desired amenities</CardTitle></CardHeader><CardContent className="flex flex-wrap gap-2">{demand.amenityNames.map((a) => <Badge key={a}>{a}</Badge>)}</CardContent></Card>
      ) : null}

      {demand.isOwner && contactResult?.ok ? (
        <Card>
          <CardHeader><CardTitle>Client contact (private — owner only)</CardTitle></CardHeader>
          <CardContent className="text-sm text-foreground">
            {contactResult.data.clientName || contactResult.data.phone || contactResult.data.email ? (
              <>
                {contactResult.data.clientName ? <p>{contactResult.data.clientName}</p> : null}
                {contactResult.data.phone ? <p>{contactResult.data.phone}</p> : null}
                {contactResult.data.email ? <p>{contactResult.data.email}</p> : null}
                {contactResult.data.confidentialNotes ? <p className="mt-2 text-muted-foreground">{contactResult.data.confidentialNotes}</p> : null}
              </>
            ) : (
              <p className="text-muted-foreground">No client contact information recorded.</p>
            )}
          </CardContent>
        </Card>
      ) : null}

      {demand.isOwner ? (
        <Card>
          <CardHeader><CardTitle>Manage this requirement</CardTitle></CardHeader>
          <CardContent className="flex flex-wrap items-center gap-2">
            <Button href={`/portal/demands/${id}/edit`} variant="secondary" size="sm">Edit</Button>

            {(demand.status === "Draft" || demand.status === "Archived") ? (
              <form action={publishDemandAction.bind(null, id)}>
                <SubmitButton size="sm">Publish</SubmitButton>
              </form>
            ) : null}

            {demand.status === "Active" ? (
              <form action={fulfillDemandAction.bind(null, id)}>
                <SubmitButton size="sm" variant="secondary">Mark fulfilled</SubmitButton>
              </form>
            ) : null}

            {demand.status !== "Archived" ? (
              <form action={archiveDemandAction.bind(null, id)}>
                <ConfirmSubmitButton confirmMessage="Archive this requirement?" variant="secondary">Archive</ConfirmSubmitButton>
              </form>
            ) : null}

            <form action={deleteDemandAction.bind(null, id)}>
              <ConfirmSubmitButton confirmMessage="Delete this requirement? This cannot be undone." variant="destructive">
                <Trash2 className="h-4 w-4" /> Delete
              </ConfirmSubmitButton>
            </form>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-foreground">{value}</p>
    </div>
  );
}
