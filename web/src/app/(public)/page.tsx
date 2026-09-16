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

export default function HomePage() {
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
      />
      <ContentFeedSection
        eyebrow="Updates"
        title="Notices"
        icon={Bell}
        emptyTitle="No notices published yet"
        emptyDescription="Official notices from the association will appear here once published."
        viewAllHref="/notices"
        viewAllLabel="View all notices"
      />
      <ContentFeedSection
        eyebrow="Updates"
        title="Events"
        icon={CalendarDays}
        emptyTitle="No events published yet"
        emptyDescription="Upcoming association events will appear here once published."
        viewAllHref="/events"
        viewAllLabel="View all events"
      />
      <MembershipCta />
      <PublicPropertiesSection />
      <ContactCta />
    </>
  );
}
