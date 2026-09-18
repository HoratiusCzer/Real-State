import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { publicContentApi } from "@/lib/public-content/api";

export async function generateMetadata({ params }: { params: Promise<{ slug: string }> }): Promise<Metadata> {
  const { slug } = await params;
  const result = await publicContentApi.getNotice(slug);
  if (!result.ok) return { title: "Notice" };

  const { title } = result.data;
  return {
    title,
    alternates: { canonical: `/notices/${slug}` },
    openGraph: { title, type: "article", url: `/notices/${slug}` },
  };
}

export default async function NoticePage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const result = await publicContentApi.getNotice(slug);
  if (!result.ok) notFound();
  const notice = result.data;

  return (
    <article className="mx-auto max-w-3xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">{notice.title}</h1>
        <p className="mt-2 text-sm text-muted-foreground">{new Date(notice.publishedAt).toLocaleDateString()}</p>
      </div>
      {notice.body ? <div className="whitespace-pre-wrap text-sm leading-relaxed text-foreground">{notice.body}</div> : null}
    </article>
  );
}
