import type { Metadata } from "next";
import { Bell, CalendarDays, Newspaper } from "lucide-react";
import { Hero } from "@/components/marketing/hero";
import { IntroSection } from "@/components/marketing/intro-section";
import { MissionVision } from "@/components/marketing/mission-vision";
import { Benefits } from "@/components/marketing/benefits";
import { ExchangeExplainer } from "@/components/marketing/exchange-explainer";
import { HowItWorks } from "@/components/marketing/how-it-works";
import { VerifiedMembers } from "@/components/marketing/verified-members";
import { ContentFeedSection } from "@/components/marketing/content-feed-section";
import { MembershipCta } from "@/components/marketing/membership-cta";
import { PublicPropertiesSection } from "@/components/marketing/public-properties-section";
import { ContactCta } from "@/components/marketing/contact-cta";
import { publicContentApi } from "@/lib/public-content/api";

export const metadata: Metadata = { alternates: { canonical: "/" } };

export default async function HomePage() {
  const [newsResult, noticesResult, eventsResult] = await Promise.all([
    publicContentApi.listNews(),
    publicContentApi.listNotices(),
    publicContentApi.listEvents(),
  ]);

  const newsItems = (newsResult.ok ? newsResult.data : []).slice(0, 3).map((n) => ({
    href: `/news/${n.slug}`, title: n.title, dateLabel: new Date(n.publishedAt).toLocaleDateString(),
  }));
  const noticeItems = (noticesResult.ok ? noticesResult.data : []).slice(0, 3).map((n) => ({
    href: `/notices/${n.slug}`, title: n.title, dateLabel: new Date(n.publishedAt).toLocaleDateString(),
  }));
  const eventItems = (eventsResult.ok ? eventsResult.data : []).slice(0, 3).map((e) => ({
    href: `/events/${e.slug}`, title: e.title, dateLabel: e.eventDate ? new Date(e.eventDate).toLocaleDateString() : "Date TBA",
  }));

  return (
    <>
      <Hero />
      <IntroSection />
      <MissionVision />
      <Benefits />
      <ExchangeExplainer />
      <HowItWorks />
      <VerifiedMembers />
      <ContentFeedSection
        eyebrow="Updates"
        title="News"
        icon={Newspaper}
        emptyTitle="No news published yet"
        emptyDescription="Association news will appear here once published from the Admin CMS."
        viewAllHref="/news"
        viewAllLabel="View all news"
        items={newsItems}
      />
      <ContentFeedSection
        eyebrow="Updates"
        title="Notices"
        icon={Bell}
        emptyTitle="No notices published yet"
        emptyDescription="Official notices from the association will appear here once published."
        viewAllHref="/notices"
        viewAllLabel="View all notices"
        items={noticeItems}
      />
      <ContentFeedSection
        eyebrow="Updates"
        title="Events"
        icon={CalendarDays}
        emptyTitle="No events published yet"
        emptyDescription="Upcoming association events will appear here once published."
        viewAllHref="/events"
        viewAllLabel="View all events"
        items={eventItems}
      />
      <MembershipCta />
      <PublicPropertiesSection />
      <ContactCta />
    </>
  );
}
