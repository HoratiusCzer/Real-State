import type { Metadata } from "next";
import { requireSession } from "@/lib/auth/session";
import { featureFlagsApi } from "@/lib/admin/api";
import { setFeatureFlagAction } from "@/lib/admin/actions";
import { FEATURE_FLAG_INFO } from "@/lib/admin/types";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { SubmitButton } from "@/components/auth/submit-button";

export const metadata: Metadata = { title: "Feature Flags" };

export default async function AdminFeatureFlagsPage() {
  const { accessToken } = await requireSession();
  const result = await featureFlagsApi.list(accessToken);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Feature Flags</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Spec §15&apos;s 13 flags. Defaults are conservative (seeded off). A few are marked
          &quot;Reserved&quot; — the flag exists and can be toggled, but no feature in this codebase
          reads it yet, so toggling it currently has no visible effect.
        </p>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load feature flags: {result.error}</p>
      ) : (
        <ul className="space-y-2">
          {result.data.map((f) => {
            const info = FEATURE_FLAG_INFO[f.key];
            return (
              <li key={f.id}>
                <Card className="p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <p className="text-sm font-medium text-foreground">{info?.label ?? f.key}</p>
                        {info?.reserved ? <Badge variant="default">Reserved</Badge> : null}
                      </div>
                      <p className="font-mono text-xs text-muted-foreground">{f.key}</p>
                      {info ? <p className="mt-1 text-xs text-muted-foreground">{info.description}</p> : null}
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant={f.isEnabled ? "success" : "default"}>{f.isEnabled ? "Enabled" : "Disabled"}</Badge>
                      <form action={setFeatureFlagAction.bind(null, f.key, !f.isEnabled)}>
                        <SubmitButton size="sm" variant="secondary">{f.isEnabled ? "Disable" : "Enable"}</SubmitButton>
                      </form>
                    </div>
                  </div>
                </Card>
              </li>
            );
          })}
        </ul>
      )}

      <Card>
        <CardContent className="pt-6 text-sm text-muted-foreground">
          Toggling a flag is itself audited (spec §19&apos;s &quot;feature flag changes&quot; category) — see{" "}
          <a href="/admin/audit-logs" className="text-primary hover:underline">Audit Logs</a>.
        </CardContent>
      </Card>
    </div>
  );
}
