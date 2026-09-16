import type { MetadataRoute } from "next";

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

export default function sitemap(): MetadataRoute.Sitemap {
  const base = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

  return staticRoutes.map((route) => ({
    url: `${base}${route}`,
    lastModified: new Date(),
  }));
}
