import type { Metadata } from "next";
import Link from "next/link";
import { Building2 } from "lucide-react";
import { publicContentApi } from "@/lib/public-content/api";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Members", alternates: { canonical: "/members" } };

export default async function MembersPage() {
  const result = await publicContentApi.listMembers();

  if (!result.ok || !result.data.enabled) {
    return (
      <PagePlaceholder
        icon={Building2}
        title="Member directory"
        description="REAK's public member directory isn't open yet. Check back later, or log in as a member to browse the network directory."
      />
    );
  }

  const { items } = result.data;

  return (
    <section className="mx-auto max-w-5xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">Members</h1>
        <p className="mt-2 text-sm text-muted-foreground">Verified REAK member organizations.</p>
      </div>

      {items.length === 0 ? (
        <EmptyState icon={Building2} title="No members published yet" description="Verified member organizations will be listed here." />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((m) => (
            <Link key={m.id} href={`/members/${m.id}`}>
              <Card className="p-4 transition-colors hover:border-accent">
                <p className="text-sm font-medium text-foreground">{m.name}</p>
                {m.description ? <p className="mt-1 line-clamp-2 text-xs text-muted-foreground">{m.description}</p> : null}
              </Card>
            </Link>
          ))}
        </div>
      )}
    </section>
  );
}
