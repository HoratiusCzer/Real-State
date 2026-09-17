import type { Metadata } from "next";
import { BookOpen } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsResourcesApi } from "@/lib/admin/api";
import { createResourceAction, setResourceStatusAction, deleteResourceAction } from "@/lib/admin/actions";
import { CONTENT_STATUS } from "@/lib/admin/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "Resources" };

export default async function AdminResourcesPage() {
  const { accessToken } = await requireSession();
  const result = await cmsResourcesApi.list(accessToken);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Resources</h1>
        <p className="mt-1 text-sm text-muted-foreground">Documents and links published for members and the public (spec §4.3).</p>
      </div>

      <Card>
        <CardHeader><CardTitle>Add a resource</CardTitle></CardHeader>
        <CardContent>
          <form action={createResourceAction} className="space-y-4">
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" required />
            </div>
            <div>
              <Label htmlFor="description">Description</Label>
              <Textarea id="description" name="description" rows={2} />
            </div>
            <div>
              <Label htmlFor="linkUrl">Link URL</Label>
              <Input id="linkUrl" name="linkUrl" type="url" />
            </div>
            <SubmitButton>Add draft</SubmitButton>
          </form>
        </CardContent>
      </Card>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load resources: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={BookOpen} title="No resources yet" description="Add the first resource above." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((r) => (
            <li key={r.id}>
              <Card className="p-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-foreground">{r.title}</p>
                    {r.description ? <p className="text-xs text-muted-foreground">{r.description}</p> : null}
                    {r.linkUrl ? <p className="text-xs text-primary">{r.linkUrl}</p> : null}
                  </div>
                  <Badge variant={contentStatusBadgeVariant(r.status)}>{r.status}</Badge>
                </div>
                <div className="mt-3 flex flex-wrap gap-2">
                  {r.status === "Draft" ? (
                    <form action={setResourceStatusAction.bind(null, r.id, CONTENT_STATUS.Review)}>
                      <SubmitButton size="sm">Submit for review</SubmitButton>
                    </form>
                  ) : null}
                  {r.status === "Review" ? (
                    <form action={setResourceStatusAction.bind(null, r.id, CONTENT_STATUS.Published)}>
                      <SubmitButton size="sm">Publish</SubmitButton>
                    </form>
                  ) : null}
                  {r.status === "Published" ? (
                    <form action={setResourceStatusAction.bind(null, r.id, CONTENT_STATUS.Archived)}>
                      <SubmitButton size="sm" variant="destructive">Archive</SubmitButton>
                    </form>
                  ) : null}
                  {r.status === "Archived" ? (
                    <form action={setResourceStatusAction.bind(null, r.id, CONTENT_STATUS.Draft)}>
                      <SubmitButton size="sm" variant="secondary">Restore to draft</SubmitButton>
                    </form>
                  ) : null}
                  <form action={deleteResourceAction.bind(null, r.id)}>
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
