import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { requireSession } from "@/lib/auth/session";
import { cmsPagesApi } from "@/lib/admin/api";
import { updatePageAction, setPageStatusAction, deletePageAction } from "@/lib/admin/actions";
import { CONTENT_STATUS } from "@/lib/admin/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { Button } from "@/components/ui/button";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "Edit page" };

export default async function AdminPageDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();
  const result = await cmsPagesApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this page: {result.error}</p>;
  }
  const page = result.data;

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">{page.title}</h1>
          <p className="mt-1 text-sm text-muted-foreground">/{page.slug}</p>
        </div>
        <Badge variant={contentStatusBadgeVariant(page.status)}>{page.status}</Badge>
      </div>

      <Card>
        <CardHeader><CardTitle>Lifecycle</CardTitle></CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          {page.status === "Draft" ? (
            <form action={setPageStatusAction.bind(null, id, CONTENT_STATUS.Review)}>
              <SubmitButton size="sm">Submit for review</SubmitButton>
            </form>
          ) : null}
          {page.status === "Review" ? (
            <>
              <form action={setPageStatusAction.bind(null, id, CONTENT_STATUS.Published)}>
                <SubmitButton size="sm">Publish</SubmitButton>
              </form>
              <form action={setPageStatusAction.bind(null, id, CONTENT_STATUS.Draft)}>
                <SubmitButton size="sm" variant="secondary">Send back to draft</SubmitButton>
              </form>
            </>
          ) : null}
          {page.status === "Published" ? (
            <form action={setPageStatusAction.bind(null, id, CONTENT_STATUS.Archived)}>
              <SubmitButton size="sm" variant="destructive">Archive</SubmitButton>
            </form>
          ) : null}
          {page.status === "Archived" ? (
            <form action={setPageStatusAction.bind(null, id, CONTENT_STATUS.Draft)}>
              <SubmitButton size="sm" variant="secondary">Restore to draft</SubmitButton>
            </form>
          ) : null}
          {page.status === "Draft" ? (
            <form action={deletePageAction.bind(null, id)}>
              <SubmitButton size="sm" variant="destructive">Delete</SubmitButton>
            </form>
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Edit</CardTitle></CardHeader>
        <CardContent>
          <form action={updatePageAction.bind(null, id)} className="space-y-4">
            <div>
              <Label htmlFor="slug">Slug</Label>
              <Input id="slug" name="slug" defaultValue={page.slug} required />
            </div>
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" defaultValue={page.title} required />
            </div>
            <div>
              <Label htmlFor="body">Body</Label>
              <Textarea id="body" name="body" rows={10} defaultValue={page.body ?? ""} />
            </div>
            <div>
              <Label htmlFor="seoTitle">SEO title</Label>
              <Input id="seoTitle" name="seoTitle" defaultValue={page.seoTitle ?? ""} />
            </div>
            <div>
              <Label htmlFor="seoDescription">SEO description</Label>
              <Textarea id="seoDescription" name="seoDescription" rows={2} defaultValue={page.seoDescription ?? ""} />
            </div>
            <div>
              <Label htmlFor="ogImageUrl">OG image URL</Label>
              <Input id="ogImageUrl" name="ogImageUrl" type="url" defaultValue={page.ogImageUrl ?? ""} />
            </div>
            <div className="flex gap-2">
              <SubmitButton>Save changes</SubmitButton>
              <Button href="/admin/cms/pages" variant="secondary">Back to pages</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
