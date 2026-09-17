import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { requireSession } from "@/lib/auth/session";
import { cmsNoticesApi } from "@/lib/admin/api";
import { updateNoticeAction, setNoticeStatusAction, deleteNoticeAction } from "@/lib/admin/actions";
import { CONTENT_STATUS } from "@/lib/admin/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { Button } from "@/components/ui/button";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "Edit notice" };

export default async function AdminNoticeDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();
  const result = await cmsNoticesApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this notice: {result.error}</p>;
  }
  const notice = result.data;

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">{notice.title}</h1>
          <p className="mt-1 text-sm text-muted-foreground">/{notice.slug}</p>
        </div>
        <Badge variant={contentStatusBadgeVariant(notice.status)}>{notice.status}</Badge>
      </div>

      <Card>
        <CardHeader><CardTitle>Lifecycle</CardTitle></CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          {notice.status === "Draft" ? (
            <form action={setNoticeStatusAction.bind(null, id, CONTENT_STATUS.Review)}>
              <SubmitButton size="sm">Submit for review</SubmitButton>
            </form>
          ) : null}
          {notice.status === "Review" ? (
            <>
              <form action={setNoticeStatusAction.bind(null, id, CONTENT_STATUS.Published)}>
                <SubmitButton size="sm">Publish</SubmitButton>
              </form>
              <form action={setNoticeStatusAction.bind(null, id, CONTENT_STATUS.Draft)}>
                <SubmitButton size="sm" variant="secondary">Send back to draft</SubmitButton>
              </form>
            </>
          ) : null}
          {notice.status === "Published" ? (
            <form action={setNoticeStatusAction.bind(null, id, CONTENT_STATUS.Archived)}>
              <SubmitButton size="sm" variant="destructive">Archive</SubmitButton>
            </form>
          ) : null}
          {notice.status === "Archived" ? (
            <form action={setNoticeStatusAction.bind(null, id, CONTENT_STATUS.Draft)}>
              <SubmitButton size="sm" variant="secondary">Restore to draft</SubmitButton>
            </form>
          ) : null}
          {notice.status === "Draft" ? (
            <form action={deleteNoticeAction.bind(null, id)}>
              <SubmitButton size="sm" variant="destructive">Delete</SubmitButton>
            </form>
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Edit</CardTitle></CardHeader>
        <CardContent>
          <form action={updateNoticeAction.bind(null, id)} className="space-y-4">
            <div>
              <Label htmlFor="slug">Slug</Label>
              <Input id="slug" name="slug" defaultValue={notice.slug} required />
            </div>
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" defaultValue={notice.title} required />
            </div>
            <div>
              <Label htmlFor="body">Body</Label>
              <Textarea id="body" name="body" rows={10} defaultValue={notice.body ?? ""} />
            </div>
            <div className="flex gap-2">
              <SubmitButton>Save changes</SubmitButton>
              <Button href="/admin/cms/notices" variant="secondary">Back to notices</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
