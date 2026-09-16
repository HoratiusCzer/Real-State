import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Bookmark, BookmarkCheck, FileText, Trash2 } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { listingsApi } from "@/lib/listings/api";
import {
  approveListingAction,
  archiveListingAction,
  deleteDocumentAction,
  deleteListingAction,
  deleteMediaAction,
  rejectListingFormAction,
  submitListingAction,
  toggleSaveListingAction,
} from "@/lib/listings/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { ConfirmSubmitButton } from "@/components/listings/confirm-submit-button";
import { statusBadgeVariant, statusLabel } from "@/lib/listings/status-badge";

export const metadata: Metadata = { title: "Property detail" };

export default async function PropertyDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { user, accessToken } = await requireSession();

  const result = await listingsApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this property: {result.error}</p>;
  }
  const listing = result.data;

  const [savedResult, contactResult] = await Promise.all([
    listingsApi.listSaved(accessToken),
    listing.isOwner ? listingsApi.getContact(accessToken, id) : Promise.resolve(null),
  ]);
  const isSaved = savedResult.ok && savedResult.data.some((s) => s.listingId === id);
  const canModerate = user.permissions.includes("listings.moderate");

  return (
    <div className="max-w-4xl space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <h1 className="font-heading text-2xl font-bold text-foreground">{listing.title}</h1>
            <Badge variant={statusBadgeVariant(listing.status)}>{statusLabel(listing.status)}</Badge>
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            {listing.referenceCode} · {listing.memberEntityName}
          </p>
        </div>
        <form action={toggleSaveListingAction.bind(null, id, isSaved)}>
          <SubmitButton variant="secondary" size="sm">
            {isSaved ? <BookmarkCheck className="h-4 w-4" /> : <Bookmark className="h-4 w-4" />}
            {isSaved ? "Saved" : "Save"}
          </SubmitButton>
        </form>
      </div>

      {listing.status === "Rejected" && listing.rejectionReason ? (
        <Card className="border-destructive/40 bg-destructive/5 p-4">
          <p className="text-sm font-medium text-destructive">Rejected</p>
          <p className="text-sm text-muted-foreground">{listing.rejectionReason}</p>
        </Card>
      ) : null}

      {listing.media.length > 0 ? (
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
          {listing.media.map((m) => (
            <div key={m.id} className="relative">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={m.url} alt={m.caption ?? listing.title} className="aspect-square rounded-md object-cover" />
              {listing.isOwner ? (
                <form action={deleteMediaAction.bind(null, id, m.id)} className="absolute right-1 top-1">
                  <SubmitButton variant="destructive" size="sm" className="h-7 px-2">
                    <Trash2 className="h-3 w-3" />
                  </SubmitButton>
                </form>
              ) : null}
            </div>
          ))}
        </div>
      ) : null}

      <Card>
        <CardHeader>
          <CardTitle>Details</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-3">
          <Detail label="Price" value={`${listing.currencyCode} ${listing.price.toLocaleString()}${listing.isPriceNegotiable ? " (negotiable)" : ""}`} />
          <Detail label="Type" value={`${listing.propertyTypeName}${listing.propertySubtypeName ? ` / ${listing.propertySubtypeName}` : ""}`} />
          <Detail label="Purpose" value={listing.purposeName} />
          <Detail label="Land area" value={`${listing.landArea} ${listing.areaUnitName}`} />
          {listing.builtUpArea ? <Detail label="Built-up area" value={`${listing.builtUpArea} ${listing.areaUnitName}`} /> : null}
          <Detail
            label="Location"
            value={[listing.localityName, `Ward ${listing.wardNumber}`, listing.municipalityName, listing.districtName, listing.provinceName]
              .filter(Boolean)
              .join(", ")}
          />
          {listing.landmark ? <Detail label="Landmark" value={listing.landmark} /> : null}
          {listing.bedrooms != null ? <Detail label="Bedrooms" value={String(listing.bedrooms)} /> : null}
          {listing.bathrooms != null ? <Detail label="Bathrooms" value={String(listing.bathrooms)} /> : null}
          {listing.parkingSpaces != null ? <Detail label="Parking" value={String(listing.parkingSpaces)} /> : null}
          {listing.furnishing ? <Detail label="Furnishing" value={listing.furnishing} /> : null}
          {listing.facing ? <Detail label="Facing" value={listing.facing} /> : null}
          <Detail label="Road access" value={listing.hasRoadAccess ? `Yes${listing.roadWidthFeet ? ` (${listing.roadWidthFeet} ft)` : ""}` : "No"} />
        </CardContent>
      </Card>

      {listing.description ? (
        <Card>
          <CardHeader><CardTitle>Description</CardTitle></CardHeader>
          <CardContent><p className="text-sm text-foreground whitespace-pre-wrap">{listing.description}</p></CardContent>
        </Card>
      ) : null}

      {listing.amenityNames.length > 0 ? (
        <Card>
          <CardHeader><CardTitle>Amenities</CardTitle></CardHeader>
          <CardContent className="flex flex-wrap gap-2">
            {listing.amenityNames.map((a) => <Badge key={a}>{a}</Badge>)}
          </CardContent>
        </Card>
      ) : null}

      {listing.isOwner && contactResult?.ok ? (
        <Card>
          <CardHeader><CardTitle>Contact (private — owner only)</CardTitle></CardHeader>
          <CardContent className="text-sm text-foreground">
            {contactResult.data.contactName || contactResult.data.phone || contactResult.data.email ? (
              <>
                {contactResult.data.contactName ? <p>{contactResult.data.contactName}</p> : null}
                {contactResult.data.phone ? <p>{contactResult.data.phone}</p> : null}
                {contactResult.data.email ? <p>{contactResult.data.email}</p> : null}
              </>
            ) : (
              <p className="text-muted-foreground">No contact information recorded.</p>
            )}
          </CardContent>
        </Card>
      ) : null}

      {listing.isOwner && listing.documents.length > 0 ? (
        <Card>
          <CardHeader><CardTitle>Documents (private — owner only)</CardTitle></CardHeader>
          <CardContent className="space-y-2">
            {listing.documents.map((d) => (
              <div key={d.id} className="flex items-center justify-between gap-2 text-sm">
                <span className="flex items-center gap-2">
                  <FileText className="h-4 w-4 text-muted-foreground" />
                  {d.documentType ?? "Document"}
                </span>
                <div className="flex items-center gap-2">
                  <Button href={`/api/listings/${id}/documents/${d.id}/download`} variant="ghost" size="sm">
                    Download
                  </Button>
                  <form action={deleteDocumentAction.bind(null, id, d.id)}>
                    <SubmitButton variant="ghost" size="sm"><Trash2 className="h-4 w-4" /></SubmitButton>
                  </form>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      ) : null}

      {listing.isOwner ? (
        <Card>
          <CardHeader><CardTitle>Manage this listing</CardTitle></CardHeader>
          <CardContent className="flex flex-wrap items-center gap-2">
            <Button href={`/portal/properties/${id}/edit`} variant="secondary" size="sm">Edit</Button>

            {(listing.status === "Draft" || listing.status === "Rejected") ? (
              <form action={submitListingAction.bind(null, id)}>
                <SubmitButton size="sm">Submit for review</SubmitButton>
              </form>
            ) : null}

            {listing.status !== "Archived" ? (
              <form action={archiveListingAction.bind(null, id)}>
                <ConfirmSubmitButton confirmMessage="Archive this listing?" variant="secondary">Archive</ConfirmSubmitButton>
              </form>
            ) : null}

            <form action={deleteListingAction.bind(null, id)}>
              <ConfirmSubmitButton confirmMessage="Delete this listing? This cannot be undone." variant="destructive">
                <Trash2 className="h-4 w-4" /> Delete
              </ConfirmSubmitButton>
            </form>
          </CardContent>
        </Card>
      ) : null}

      {canModerate && listing.status === "PendingReview" ? (
        <Card>
          <CardHeader><CardTitle>Moderation</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            <form action={approveListingAction.bind(null, id)}>
              <SubmitButton size="sm">Approve</SubmitButton>
            </form>
            <form action={rejectListingFormAction.bind(null, id)} className="flex items-end gap-2">
              <div className="flex-1">
                <Label htmlFor="reason">Rejection reason</Label>
                <Input id="reason" name="reason" required />
              </div>
              <SubmitButton variant="destructive" size="sm">Reject</SubmitButton>
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
