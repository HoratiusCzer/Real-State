import type { Metadata } from "next";
import { Users } from "lucide-react";
import { publicContentApi } from "@/lib/public-content/api";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Leadership", alternates: { canonical: "/leadership" } };

export default async function LeadershipPage() {
  const result = await publicContentApi.listCommittee();
  const members = result.ok ? result.data : [];

  return (
    <section className="mx-auto max-w-4xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">Leadership</h1>
        <p className="mt-2 text-sm text-muted-foreground">REAK&apos;s committee.</p>
      </div>

      {members.length === 0 ? (
        <EmptyState icon={Users} title="No leadership published yet" description="Committee and leadership information will appear here once published by the association." />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {members.map((m) => (
            <Card key={m.name} className="p-4 text-center">
              {m.photoUrl ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img src={m.photoUrl} alt={m.name} className="mx-auto h-20 w-20 rounded-full object-cover" />
              ) : (
                <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-muted text-muted-foreground">
                  <Users className="h-8 w-8" aria-hidden="true" />
                </div>
              )}
              <p className="mt-3 text-sm font-medium text-foreground">{m.name}</p>
              <p className="text-xs text-muted-foreground">{m.title}</p>
            </Card>
          ))}
        </div>
      )}
    </section>
  );
}
