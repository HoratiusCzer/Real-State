import type { Metadata } from "next";
import { Settings } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsSettingsApi } from "@/lib/admin/api";
import { upsertSiteSettingAction, deleteSiteSettingAction } from "@/lib/admin/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Site Settings" };

export default async function AdminSiteSettingsPage() {
  const { accessToken } = await requireSession();
  const result = await cmsSettingsApi.list(accessToken);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Site Settings</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Generic key/value configuration (spec §4.3) — default OG image, social links, etc. Takes effect
          immediately, unlike CMS content. Never pre-seeded with REAK&apos;s real contact info or legal name;
          the association supplies that here.
        </p>
      </div>

      <Card>
        <CardHeader><CardTitle>Set a value</CardTitle></CardHeader>
        <CardContent>
          <form action={upsertSiteSettingAction} className="space-y-4">
            <div>
              <Label htmlFor="key">Key</Label>
              <Input id="key" name="key" required placeholder="social_facebook_url" />
              <p className="mt-1 text-xs text-muted-foreground">Setting an existing key updates its value.</p>
            </div>
            <div>
              <Label htmlFor="value">Value</Label>
              <Textarea id="value" name="value" rows={2} />
            </div>
            <div>
              <Label htmlFor="description">Description</Label>
              <Input id="description" name="description" />
            </div>
            <SubmitButton>Save</SubmitButton>
          </form>
        </CardContent>
      </Card>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load settings: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Settings} title="No settings configured yet" description="Add the first key/value pair above." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((s) => (
            <li key={s.id}>
              <Card className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="font-mono text-sm font-medium text-foreground">{s.key}</p>
                  <p className="truncate text-xs text-muted-foreground">{s.value ?? "—"}</p>
                  {s.description ? <p className="text-xs text-muted-foreground">{s.description}</p> : null}
                </div>
                <form action={deleteSiteSettingAction.bind(null, s.id)}>
                  <SubmitButton size="sm" variant="ghost">Delete</SubmitButton>
                </form>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
