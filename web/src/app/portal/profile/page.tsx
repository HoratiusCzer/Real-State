import type { Metadata } from "next";
import { requireSession } from "@/lib/auth/session";
import { ProfileForm } from "@/components/portal/profile-form";

export const metadata: Metadata = { title: "Profile" };

export default async function PortalProfilePage() {
  const { user } = await requireSession();

  return (
    <div className="max-w-md space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Profile</h1>
        <p className="mt-1 text-sm text-muted-foreground">{user.profile.email}</p>
      </div>
      <ProfileForm fullName={user.profile.fullName} phone={user.profile.phone} />
    </div>
  );
}
