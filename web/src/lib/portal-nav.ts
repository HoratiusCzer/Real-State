import type { LucideIcon } from "lucide-react";
import {
  LayoutDashboard,
  Building2,
  Home,
  ClipboardList,
  Sparkles,
  Handshake,
  Bookmark,
  Users,
  Bell,
  UserCircle,
  Building,
  Settings,
} from "lucide-react";

export type PortalNavItem = {
  href: string;
  label: string;
  icon: LucideIcon;
};

/** Spec §4.2's full Member Portal route list. Items whose owning stage (6-9) hasn't shipped yet
 * still link to a real route — it just renders a "not yet available" state there — rather than
 * being hidden or disabled, so the portal's shape is visible even before every feature exists. */
export const portalNavItems: PortalNavItem[] = [
  { href: "/portal/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/portal/properties", label: "Properties", icon: Building2 },
  { href: "/portal/my-properties", label: "My Properties", icon: Home },
  { href: "/portal/demands", label: "Requirements", icon: ClipboardList },
  { href: "/portal/matches", label: "Matches", icon: Sparkles },
  { href: "/portal/collaborations", label: "Collaborations", icon: Handshake },
  { href: "/portal/saved", label: "Saved", icon: Bookmark },
  { href: "/portal/members", label: "Members", icon: Users },
  { href: "/portal/notifications", label: "Notifications", icon: Bell },
  { href: "/portal/profile", label: "Profile", icon: UserCircle },
  { href: "/portal/organization", label: "Organization", icon: Building },
  { href: "/portal/settings", label: "Settings", icon: Settings },
];
