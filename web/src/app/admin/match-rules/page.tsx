import type { Metadata } from "next";
import Link from "next/link";
import { Sparkles } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { adminMatchRuleSetsApi } from "@/lib/admin/api";
import { createMatchRuleSetAction } from "@/lib/admin/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Match Rules" };

function statusBadgeVariant(status: string): "default" | "success" | "destructive" | "accent" {
  switch (status) {
    case "Published": return "success";
    case "Archived": return "destructive";
    default: return "accent";
  }
}

export default async function AdminMatchRulesPage() {
  const { accessToken } = await requireSession();
  const result = await adminMatchRuleSetsApi.list(accessToken);

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Match Rules</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Rule sets for the matching engine (spec §12) — no hardcoded weights. Publishing a new version
          automatically archives the previously-published one with the same name.
        </p>
      </div>

      <Card>
        <CardHeader><CardTitle>New rule set</CardTitle></CardHeader>
        <CardContent>
          <form action={createMatchRuleSetAction} className="space-y-4">
            <div>
              <Label htmlFor="name">Name</Label>
              <Input id="name" name="name" required placeholder="Standard" />
              <p className="mt-1 text-xs text-muted-foreground">Creating another rule set with the same name starts a new version.</p>
            </div>
            <div>
              <Label htmlFor="description">Description</Label>
              <Textarea id="description" name="description" rows={2} />
            </div>
            <SubmitButton>Create draft</SubmitButton>
          </form>
        </CardContent>
      </Card>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load rule sets: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Sparkles} title="No rule sets yet" description="Create one above — matching stays disabled until a rule set is published." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((s) => (
            <li key={s.id}>
              <Link href={`/admin/match-rules/${s.id}`}>
                <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-foreground">{s.name} v{s.version}</p>
                    <p className="text-xs text-muted-foreground">{s.rules.length} rule{s.rules.length === 1 ? "" : "s"}</p>
                  </div>
                  <Badge variant={statusBadgeVariant(s.status)}>{s.status}</Badge>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
