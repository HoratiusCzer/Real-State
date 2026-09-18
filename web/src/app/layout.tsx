import type { Metadata } from "next";
import { Lexend, Source_Sans_3 } from "next/font/google";
import "./globals.css";

const heading = Lexend({
  variable: "--font-heading",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

const body = Source_Sans_3({
  variable: "--font-body",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";
const siteDescription =
  "REAK is a membership-based real estate association platform connecting member companies through a shared property exchange, client requirements, and collaboration.";

export const metadata: Metadata = {
  // Required for Next.js to resolve relative canonical/OG URLs to absolute ones (spec §26) —
  // every page below can now set alternates.canonical: "/some/path" instead of a full URL.
  metadataBase: new URL(siteUrl),
  title: {
    default: "REAK",
    template: "%s | REAK",
  },
  description: siteDescription,
  openGraph: {
    siteName: "REAK",
    type: "website",
    locale: "en_US",
    title: "REAK",
    description: siteDescription,
  },
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="en"
      className={`${heading.variable} ${body.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col font-sans">{children}</body>
    </html>
  );
}
