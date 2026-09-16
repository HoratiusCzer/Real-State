import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { requireSession } from "@/lib/auth/session";
import { demandsApi } from "@/lib/demands/api";
import { DemandEditForm } from "@/components/demands/demand-edit-form";

export const metadata: Metadata = { title: "Edit requirement" };

export default async function EditDemandPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();

  const result = await demandsApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this requirement: {result.error}</p>;
  }

  if (!result.data.isOwner) {
    return <p className="text-sm text-destructive">You don&apos;t have permission to edit this requirement.</p>;
  }

  return (
    <div className="max-w-3xl space-y-6">
      <h1 className="font-heading text-2xl font-bold text-foreground">Edit requirement</h1>
      <DemandEditForm demand={result.data} initialPropertyTypeIds={result.data.propertyTypeIds} />
    </div>
  );
}
