import type { Metadata } from "next";
import { UserSquare2 } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsCommitteeApi } from "@/lib/admin/api";
import { createCommitteeMemberAction, toggleCommitteeMemberActiveAction, deleteCommitteeMemberAction } from "@/lib/admin/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Committee" };

export default async function AdminCommitteePage() {
  const { accessToken } = await requireSession();
  const result = await cmsCommitteeApi.list(accessToken);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Committee</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Leadership roster shown on the public site (spec §4.3). Structure only — never seeded with invented names.
        </p>
      </div>

      <Card>
        <CardHeader><CardTitle>Add a committee member</CardTitle></CardHeader>
        <CardContent>
          <form action={createCommitteeMemberAction} className="space-y-4">
            <div>
              <Label htmlFor="name">Name</Label>
              <Input id="name" name="name" required />
            </div>
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" required placeholder="Chairperson" />
            </div>
            <div>
              <Label htmlFor="photoUrl">Photo URL</Label>
              <Input id="photoUrl" name="photoUrl" type="url" />
            </div>
            <div>
              <Label htmlFor="sortOrder">Sort order</Label>
              <Input id="sortOrder" name="sortOrder" type="number" defaultValue={0} />
            </div>
            <SubmitButton>Add</SubmitButton>
          </form>
        </CardContent>
      </Card>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load committee members: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={UserSquare2} title="No committee members yet" description="Add the first entry above." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((c) => (
            <li key={c.id}>
              <Card className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">{c.name}</p>
                  <p className="text-xs text-muted-foreground">{c.title}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant={c.isActive ? "success" : "default"}>{c.isActive ? "Visible" : "Hidden"}</Badge>
                  <form action={toggleCommitteeMemberActiveAction.bind(null, c.id)}>
                    <SubmitButton size="sm" variant="secondary">{c.isActive ? "Hide" : "Show"}</SubmitButton>
                  </form>
                  <form action={deleteCommitteeMemberAction.bind(null, c.id)}>
                    <SubmitButton size="sm" variant="ghost">Delete</SubmitButton>
                  </form>
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
