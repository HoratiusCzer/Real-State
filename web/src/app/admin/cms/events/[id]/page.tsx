import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { requireSession } from "@/lib/auth/session";
import { cmsEventsApi } from "@/lib/admin/api";
import { updateEventAction, setEventStatusAction, deleteEventAction } from "@/lib/admin/actions";
import { CONTENT_STATUS } from "@/lib/admin/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { Button } from "@/components/ui/button";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "Edit event" };

function toLocalInputValue(iso: string | null): string {
  if (!iso) return "";
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export default async function AdminEventDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();
  const result = await cmsEventsApi.get(accessToken, id);
  if (!result.ok) {
    if (result.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this event: {result.error}</p>;
  }
  const evt = result.data;

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">{evt.title}</h1>
          <p className="mt-1 text-sm text-muted-foreground">/{evt.slug}</p>
        </div>
        <Badge variant={contentStatusBadgeVariant(evt.status)}>{evt.status}</Badge>
      </div>

      <Card>
        <CardHeader><CardTitle>Lifecycle</CardTitle></CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          {evt.status === "Draft" ? (
            <form action={setEventStatusAction.bind(null, id, CONTENT_STATUS.Review)}>
              <SubmitButton size="sm">Submit for review</SubmitButton>
            </form>
          ) : null}
          {evt.status === "Review" ? (
            <>
              <form action={setEventStatusAction.bind(null, id, CONTENT_STATUS.Published)}>
                <SubmitButton size="sm">Publish</SubmitButton>
              </form>
              <form action={setEventStatusAction.bind(null, id, CONTENT_STATUS.Draft)}>
                <SubmitButton size="sm" variant="secondary">Send back to draft</SubmitButton>
              </form>
            </>
          ) : null}
          {evt.status === "Published" ? (
            <form action={setEventStatusAction.bind(null, id, CONTENT_STATUS.Archived)}>
              <SubmitButton size="sm" variant="destructive">Archive</SubmitButton>
            </form>
          ) : null}
          {evt.status === "Archived" ? (
            <form action={setEventStatusAction.bind(null, id, CONTENT_STATUS.Draft)}>
              <SubmitButton size="sm" variant="secondary">Restore to draft</SubmitButton>
            </form>
          ) : null}
          {evt.status === "Draft" ? (
            <form action={deleteEventAction.bind(null, id)}>
              <SubmitButton size="sm" variant="destructive">Delete</SubmitButton>
            </form>
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Edit</CardTitle></CardHeader>
        <CardContent>
          <form action={updateEventAction.bind(null, id)} className="space-y-4">
            <div>
              <Label htmlFor="slug">Slug</Label>
              <Input id="slug" name="slug" defaultValue={evt.slug} required />
            </div>
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" defaultValue={evt.title} required />
            </div>
            <div>
              <Label htmlFor="eventDate">Date &amp; time</Label>
              <Input id="eventDate" name="eventDate" type="datetime-local" defaultValue={toLocalInputValue(evt.eventDate)} />
            </div>
            <div>
              <Label htmlFor="location">Location</Label>
              <Input id="location" name="location" defaultValue={evt.location ?? ""} />
            </div>
            <div>
              <Label htmlFor="body">Body</Label>
              <Textarea id="body" name="body" rows={10} defaultValue={evt.body ?? ""} />
            </div>
            <div className="flex gap-2">
              <SubmitButton>Save changes</SubmitButton>
              <Button href="/admin/cms/events" variant="secondary">Back to events</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
