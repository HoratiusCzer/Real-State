import { publicContentApi } from "@/lib/public-content/api";
import { PagePlaceholder } from "./page-placeholder";
import type { LucideIcon } from "lucide-react";

/** Generic renderer for a CmsPage by slug (spec §4.3: about, privacy, terms, and other static
 * pages all move through the same Draft -> Review -> Published -> Archived lifecycle) — falls back
 * to the same elegant empty state used before any of this existed rather than a broken page when
 * nothing's published yet. */
export async function CmsPageContent({ slug, fallbackTitle, fallbackDescription, icon }: { slug: string; fallbackTitle: string; fallbackDescription: string; icon?: LucideIcon }) {
  const result = await publicContentApi.getPage(slug);

  if (!result.ok) {
    return <PagePlaceholder icon={icon} title={fallbackTitle} description={fallbackDescription} />;
  }

  const page = result.data;
  return (
    <article className="mx-auto max-w-3xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <h1 className="font-heading text-3xl font-bold text-foreground">{page.title}</h1>
      {page.body ? <div className="whitespace-pre-wrap text-sm leading-relaxed text-foreground">{page.body}</div> : null}
    </article>
  );
}
