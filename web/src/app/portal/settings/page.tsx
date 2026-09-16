import type { Metadata } from "next";
import { ChangePasswordForm } from "@/components/portal/change-password-form";

export const metadata: Metadata = { title: "Settings" };

export default function PortalSettingsPage() {
  return (
    <div className="max-w-md space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Settings</h1>
        <p className="mt-1 text-sm text-muted-foreground">Manage your account security.</p>
      </div>
      <ChangePasswordForm />
    </div>
  );
}
