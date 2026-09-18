import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { publicContentApi } from "@/lib/public-content/api";

export async function generateMetadata({ params }: { params: Promise<{ slug: string }> }): Promise<Metadata> {
  const { slug } = await params;
  const result = await publicContentApi.getNews(slug);
  if (!result.ok) return { title: "News article" };

  const { title, summary } = result.data;
  return {
    title,
    description: summary ?? undefined,
    alternates: { canonical: `/news/${slug}` },
    openGraph: { title, description: summary ?? undefined, type: "article", url: `/news/${slug}` },
  };
}

export default async function NewsArticlePage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const result = await publicContentApi.getNews(slug);
  if (!result.ok) notFound();
  const article = result.data;

  return (
    <article className="mx-auto max-w-3xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">{article.title}</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          {article.authorName} · {new Date(article.publishedAt).toLocaleDateString()}
        </p>
      </div>
      {article.body ? <div className="whitespace-pre-wrap text-sm leading-relaxed text-foreground">{article.body}</div> : null}
    </article>
  );
}
