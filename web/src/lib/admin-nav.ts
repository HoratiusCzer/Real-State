import type { LucideIcon } from "lucide-react";
import {
  LayoutDashboard,
  ClipboardCheck,
  Mail,
  Building2,
  Users,
  ShieldCheck,
  Building,
  Sparkles,
  Handshake,
  FileText,
  Newspaper,
  Bell,
  CalendarDays,
  BookOpen,
  UserSquare2,
  Navigation as NavigationIcon,
  Settings,
  Database,
  Flag,
  BarChart3,
  History,
  Lock,
} from "lucide-react";

export type AdminNavItem = { href: string; label: string; icon: LucideIcon };
export type AdminNavSection = { heading: string; items: AdminNavItem[] };

/** Spec §4.3's full Admin Portal route list. Items whose backing feature is explicitly deferred
 * (reference data, feature flags, reports, audit logs, security — see DEVELOPMENT_PLAN.md's Stage
 * 11 section for why each one specifically) still link to a real route, same "shape is visible
 * even before the feature exists" convention as the Member Portal's own nav. */
export const adminNavSections: AdminNavSection[] = [
  {
    heading: "Overview",
    items: [{ href: "/admin", label: "Dashboard", icon: LayoutDashboard }],
  },
  {
    heading: "Members",
    items: [
      { href: "/admin/membership-applications", label: "Membership Applications", icon: ClipboardCheck },
      { href: "/admin/invitations", label: "Invitations", icon: Mail },
      { href: "/admin/members", label: "Member Organizations", icon: Building2 },
      { href: "/admin/users", label: "Users", icon: Users },
      { href: "/admin/roles", label: "Roles & Permissions", icon: ShieldCheck },
    ],
  },
  {
    heading: "Exchange",
    items: [
      { href: "/admin/listings", label: "Property Moderation", icon: Building },
      { href: "/admin/match-rules", label: "Match Rules", icon: Sparkles },
      { href: "/admin/collaborations", label: "Collaborations", icon: Handshake },
    ],
  },
  {
    heading: "CMS",
    items: [
      { href: "/admin/cms/pages", label: "Pages", icon: FileText },
      { href: "/admin/cms/news", label: "News", icon: Newspaper },
      { href: "/admin/cms/notices", label: "Notices", icon: Bell },
      { href: "/admin/cms/events", label: "Events", icon: CalendarDays },
      { href: "/admin/cms/resources", label: "Resources", icon: BookOpen },
      { href: "/admin/cms/committee", label: "Committee", icon: UserSquare2 },
      { href: "/admin/cms/navigation", label: "Navigation", icon: NavigationIcon },
      { href: "/admin/cms/settings", label: "Site Settings", icon: Settings },
    ],
  },
  {
    heading: "System",
    items: [
      { href: "/admin/reference-data", label: "Reference Data", icon: Database },
      { href: "/admin/feature-flags", label: "Feature Flags", icon: Flag },
      { href: "/admin/reports", label: "Reports", icon: BarChart3 },
      { href: "/admin/audit-logs", label: "Audit Logs", icon: History },
      { href: "/admin/security", label: "Security", icon: Lock },
    ],
  },
];
