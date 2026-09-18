import type { MetadataRoute } from "next";
import { publicContentApi } from "@/lib/public-content/api";
import { publicPropertiesApi } from "@/lib/listings/api";

const staticRoutes = [
  "",
  "/about",
  "/leadership",
  "/members",
  "/membership",
  "/membership/apply",
  "/verify-member",
  "/news",
  "/notices",
  "/events",
  "/resources",
  "/contact",
  "/properties",
  "/privacy",
  "/terms",
];

// A real sitemap needs the actual indexable pages, not just the static shell (spec §26) — every
// one of these detail routes is genuinely public and was previously missing entirely. Capped at
// one page of properties (REAK.Api's own default page size) rather than paginating exhaustively;
// a catalog large enough to need more than that is a later problem, not a Stage 14 one.
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const base = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

  const staticEntries: MetadataRoute.Sitemap = staticRoutes.map((route) => ({
    url: `${base}${route}`,
    lastModified: new Date(),
  }));

  const [newsResult, noticesResult, eventsResult, membersResult, propertiesResult] = await Promise.all([
    publicContentApi.listNews(),
    publicContentApi.listNotices(),
    publicContentApi.listEvents(),
    publicContentApi.listMembers(),
    publicPropertiesApi.search({ page: 1, pageSize: 100 }),
  ]);

  const newsEntries: MetadataRoute.Sitemap = newsResult.ok
    ? newsResult.data.map((n) => ({ url: `${base}/news/${n.slug}`, lastModified: new Date(n.publishedAt) }))
    : [];
  const noticeEntries: MetadataRoute.Sitemap = noticesResult.ok
    ? noticesResult.data.map((n) => ({ url: `${base}/notices/${n.slug}`, lastModified: new Date(n.publishedAt) }))
    : [];
  const eventEntries: MetadataRoute.Sitemap = eventsResult.ok
    ? eventsResult.data.map((e) => ({ url: `${base}/events/${e.slug}` }))
    : [];
  const memberEntries: MetadataRoute.Sitemap =
    membersResult.ok && membersResult.data.enabled
      ? membersResult.data.items.map((m) => ({ url: `${base}/members/${m.id}` }))
      : [];
  const propertyEntries: MetadataRoute.Sitemap =
    propertiesResult.ok && propertiesResult.data.enabled
      ? propertiesResult.data.items.map((p) => ({ url: `${base}/properties/${p.id}` }))
      : [];

  return [...staticEntries, ...newsEntries, ...noticeEntries, ...eventEntries, ...memberEntries, ...propertyEntries];
}
