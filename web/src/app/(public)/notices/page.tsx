import type { Metadata } from "next";
import Link from "next/link";
import { Bell } from "lucide-react";
import { publicContentApi } from "@/lib/public-content/api";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Notices", alternates: { canonical: "/notices" } };

export default async function NoticesPage() {
  const result = await publicContentApi.listNotices();
  const items = result.ok ? result.data : [];

  return (
    <section className="mx-auto max-w-4xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">Notices</h1>
        <p className="mt-2 text-sm text-muted-foreground">Official notices from the association.</p>
      </div>

      {items.length === 0 ? (
        <EmptyState icon={Bell} title="No notices published yet" description="Official association notices will appear here once published from the Admin CMS." />
      ) : (
        <ul className="space-y-3">
          {items.map((n) => (
            <li key={n.slug}>
              <Link href={`/notices/${n.slug}`}>
                <Card className="p-4 transition-colors hover:border-accent">
                  <p className="text-sm font-medium text-foreground">{n.title}</p>
                  <p className="mt-1 text-xs text-muted-foreground">{new Date(n.publishedAt).toLocaleDateString()}</p>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
