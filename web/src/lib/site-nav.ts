export const primaryNav = [
  { label: "About", href: "/about" },
  { label: "Members", href: "/members" },
  { label: "Properties", href: "/properties" },
  { label: "News", href: "/news" },
  { label: "Events", href: "/events" },
  { label: "Membership", href: "/membership" },
  { label: "Contact", href: "/contact" },
] as const;

export const footerColumns = [
  {
    heading: "Association",
    links: [
      { label: "About REAK", href: "/about" },
      { label: "Leadership", href: "/leadership" },
      { label: "Members", href: "/members" },
      { label: "Membership", href: "/membership" },
      { label: "Apply for membership", href: "/membership/apply" },
      { label: "Verify a member", href: "/verify-member" },
    ],
  },
  {
    heading: "Resources",
    links: [
      { label: "News", href: "/news" },
      { label: "Notices", href: "/notices" },
      { label: "Events", href: "/events" },
      { label: "Resources", href: "/resources" },
      { label: "Property exchange", href: "/properties" },
    ],
  },
  {
    heading: "Legal",
    links: [
      { label: "Privacy policy", href: "/privacy" },
      { label: "Terms of use", href: "/terms" },
      { label: "Contact", href: "/contact" },
    ],
  },
] as const;
