import type { Metadata } from "next";
import { FileText } from "lucide-react";
import { publicContentApi } from "@/lib/public-content/api";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Resources", alternates: { canonical: "/resources" } };

export default async function ResourcesPage() {
  const result = await publicContentApi.listResources();
  const items = result.ok ? result.data : [];

  return (
    <section className="mx-auto max-w-4xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">Resources</h1>
        <p className="mt-2 text-sm text-muted-foreground">Documents and links published by the association.</p>
      </div>

      {items.length === 0 ? (
        <EmptyState icon={FileText} title="No resources published yet" description="Association resources will appear here once published from the Admin CMS." />
      ) : (
        <ul className="space-y-3">
          {items.map((r) => (
            <li key={r.id}>
              <Card className="p-4">
                <p className="text-sm font-medium text-foreground">{r.title}</p>
                {r.description ? <p className="mt-1 text-sm text-muted-foreground">{r.description}</p> : null}
                {r.linkUrl ? (
                  <a href={r.linkUrl} target="_blank" rel="noopener noreferrer" className="mt-1 inline-block text-sm text-primary hover:underline">
                    {r.linkUrl}
                  </a>
                ) : null}
              </Card>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
