import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { requireSession } from "@/lib/auth/session";
import { adminMatchRuleSetsApi } from "@/lib/admin/api";
import { addMatchRuleAction, removeMatchRuleAction, publishMatchRuleSetAction, archiveMatchRuleSetAction } from "@/lib/admin/actions";
import { MATCH_CRITERION_LABELS } from "@/lib/matching/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = { title: "Rule set detail" };

export default async function AdminMatchRuleSetDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();
  const result = await adminMatchRuleSetsApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this rule set: {result.error}</p>;
  }
  const set = result.data;
  const isDraft = set.status === "Draft";

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">{set.name} v{set.version}</h1>
          {set.description ? <p className="mt-1 text-sm text-muted-foreground">{set.description}</p> : null}
        </div>
        <Badge variant={set.status === "Published" ? "success" : set.status === "Archived" ? "destructive" : "accent"}>{set.status}</Badge>
      </div>

      <Card>
        <CardHeader><CardTitle>Rules</CardTitle></CardHeader>
        <CardContent className="space-y-3">
          {set.rules.length === 0 ? (
            <p className="text-sm text-muted-foreground">No rules yet — add at least one with a non-zero weight before publishing.</p>
          ) : (
            <ul className="space-y-2">
              {set.rules.map((r) => (
                <li key={r.id} className="flex items-center justify-between gap-4 rounded-md border border-border p-3">
                  <div className="text-sm text-foreground">
                    <span className="font-medium">{MATCH_CRITERION_LABELS[r.criterion] ?? r.criterion}</span>{" "}
                    <span className="text-muted-foreground">
                      · weight {r.weight}{r.isRequired ? " · required" : ""}{r.toleranceValue ? ` · tolerance ${r.toleranceValue}%` : ""}
                    </span>
                  </div>
                  {isDraft ? (
                    <form action={removeMatchRuleAction.bind(null, id, r.id)}>
                      <SubmitButton size="sm" variant="ghost">Remove</SubmitButton>
                    </form>
                  ) : null}
                </li>
              ))}
            </ul>
          )}

          {isDraft ? (
            <form action={addMatchRuleAction.bind(null, id)} className="space-y-3 border-t border-border pt-4">
              <div>
                <Label htmlFor="criterion">Criterion</Label>
                <select id="criterion" name="criterion" required className="flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground">
                  {Object.entries(MATCH_CRITERION_LABELS).map(([value, label]) => (
                    <option key={value} value={value}>{label}</option>
                  ))}
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <Label htmlFor="weight">Weight</Label>
                  <Input id="weight" name="weight" type="number" step="0.1" min="0" required />
                </div>
                <div>
                  <Label htmlFor="toleranceValue">Tolerance %</Label>
                  <Input id="toleranceValue" name="toleranceValue" type="number" step="0.1" min="0" />
                </div>
              </div>
              <div className="flex items-center gap-2">
                <input id="isRequired" name="isRequired" type="checkbox" className="h-4 w-4" />
                <Label htmlFor="isRequired" className="mb-0">Required (failing this excludes the pair entirely)</Label>
              </div>
              <div>
                <Label htmlFor="sortOrder">Sort order</Label>
                <Input id="sortOrder" name="sortOrder" type="number" defaultValue={set.rules.length} />
              </div>
              <SubmitButton size="sm">Add rule</SubmitButton>
            </form>
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Lifecycle</CardTitle></CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          {isDraft ? (
            <form action={publishMatchRuleSetAction.bind(null, id)}>
              <SubmitButton size="sm">Publish</SubmitButton>
            </form>
          ) : null}
          {set.status !== "Archived" ? (
            <form action={archiveMatchRuleSetAction.bind(null, id)}>
              <SubmitButton size="sm" variant="destructive">Archive</SubmitButton>
            </form>
          ) : null}
        </CardContent>
      </Card>

      <Button href="/admin/match-rules" variant="secondary" size="sm">Back to match rules</Button>
    </div>
  );
}
