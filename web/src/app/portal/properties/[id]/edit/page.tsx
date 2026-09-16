import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { requireSession } from "@/lib/auth/session";
import { listingsApi } from "@/lib/listings/api";
import { PropertyEditForm } from "@/components/listings/property-edit-form";

export const metadata: Metadata = { title: "Edit property" };

export default async function EditPropertyPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();

  const result = await listingsApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this property: {result.error}</p>;
  }

  if (!result.data.isOwner) {
    return <p className="text-sm text-destructive">You don&apos;t have permission to edit this listing.</p>;
  }

  return (
    <div className="max-w-3xl space-y-6">
      <h1 className="font-heading text-2xl font-bold text-foreground">Edit property</h1>
      <PropertyEditForm listing={result.data} initialAmenityIds={result.data.amenityIds} />
    </div>
  );
}
