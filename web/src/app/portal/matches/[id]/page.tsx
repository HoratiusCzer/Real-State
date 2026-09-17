import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { CheckCircle2, Circle, XCircle, MinusCircle, Bookmark, X, RotateCcw, Handshake, Flag } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { matchesApi } from "@/lib/matching/api";
import { recordMatchActionAction } from "@/lib/matching/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { SubmitButton } from "@/components/auth/submit-button";
import { matchStatusBadgeVariant } from "@/lib/matching/status-badge";
import { MATCH_CRITERION_LABELS, MATCH_RESULT_LABELS } from "@/lib/matching/types";

export const metadata: Metadata = { title: "Match detail" };

const RESULT_ICON: Record<number, React.ReactNode> = {
  1: <CheckCircle2 className="h-4 w-4 text-success" aria-hidden="true" />,
  2: <Circle className="h-4 w-4 text-amber-500" aria-hidden="true" />,
  3: <XCircle className="h-4 w-4 text-destructive" aria-hidden="true" />,
  4: <MinusCircle className="h-4 w-4 text-muted-foreground" aria-hidden="true" />,
};

export default async function MatchDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();

  const result = await matchesApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    if (result.status === 403) return <p className="text-sm text-destructive">You don&apos;t have permission to view this match.</p>;
    return <p className="text-sm text-destructive">Couldn&apos;t load this match: {result.error}</p>;
  }
  const match = result.data;

  return (
    <div className="max-w-3xl space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">
            {match.listingTitle} <span className="text-muted-foreground">×</span> {match.demandTitle}
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {match.listingReferenceCode} ({match.listingMemberEntityName}) &middot; {match.demandReferenceCode} ({match.demandMemberEntityName})
          </p>
        </div>
        <div className="text-right">
          <p className="font-heading text-3xl font-bold text-foreground">{match.score.toFixed(0)}%</p>
          <Badge variant={matchStatusBadgeVariant(match.status)}>{match.status}</Badge>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Explanation</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="mb-4 text-xs text-muted-foreground">
            Rule set &quot;{match.matchRuleSetName}&quot; v{match.matchRuleSetVersion} &middot; computed {new Date(match.computedAt).toLocaleString()}
          </p>
          <ul className="space-y-3">
            {match.components.map((c) => (
              <li key={c.criterion} className="flex items-start gap-3">
                {RESULT_ICON[c.result]}
                <div>
                  <p className="text-sm font-medium text-foreground">
                    {MATCH_CRITERION_LABELS[c.criterion] ?? c.criterion} <span className="text-muted-foreground">— {MATCH_RESULT_LABELS[c.result]}</span>
                  </p>
                  {c.detailText ? <p className="text-xs text-muted-foreground">{c.detailText}</p> : null}
                </div>
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Actions</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          <form action={recordMatchActionAction.bind(null, id, 1)}>
            <SubmitButton size="sm" variant="secondary"><Bookmark className="h-4 w-4" /> Shortlist</SubmitButton>
          </form>
          <form action={recordMatchActionAction.bind(null, id, 2)}>
            <SubmitButton size="sm" variant="secondary"><X className="h-4 w-4" /> Dismiss</SubmitButton>
          </form>
          <form action={recordMatchActionAction.bind(null, id, 3)}>
            <SubmitButton size="sm" variant="secondary"><RotateCcw className="h-4 w-4" /> Reopen</SubmitButton>
          </form>
          <form action={recordMatchActionAction.bind(null, id, 4)}>
            <SubmitButton size="sm"><Handshake className="h-4 w-4" /> Request collaboration</SubmitButton>
          </form>
          <form action={recordMatchActionAction.bind(null, id, 5)}>
            <SubmitButton size="sm" variant="destructive"><Flag className="h-4 w-4" /> Report incorrect data</SubmitButton>
          </form>
        </CardContent>
      </Card>

      {match.actions.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Activity</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {match.actions.map((a) => (
              <div key={a.id} className="text-sm text-foreground">
                <span className="font-medium">{a.byProfileName}</span>{" "}
                <span className="text-muted-foreground">
                  {actionLabel(a.actionType)} · {new Date(a.createdAt).toLocaleString()}
                </span>
                {a.notes ? <p className="text-xs text-muted-foreground">{a.notes}</p> : null}
              </div>
            ))}
          </CardContent>
        </Card>
      ) : null}

      <Button href="/portal/matches" variant="secondary" size="sm">
        Back to matches
      </Button>
    </div>
  );
}

function actionLabel(actionType: number): string {
  switch (actionType) {
    case 1: return "shortlisted";
    case 2: return "dismissed";
    case 3: return "reopened";
    case 4: return "requested collaboration";
    case 5: return "reported incorrect data";
    default: return "acted";
  }
}
