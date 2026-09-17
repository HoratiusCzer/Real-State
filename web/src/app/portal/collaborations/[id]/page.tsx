import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { MessageSquare, StickyNote, ListChecks, CalendarClock, Paperclip, History, ShieldCheck, Trash2 } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { collaborationsApi } from "@/lib/collaboration/api";
import {
  sendMessageAction,
  addNoteAction,
  createTaskAction,
  updateTaskStatusAction,
  scheduleViewingAction,
  uploadFileAction,
  deleteFileAction,
  grantContactDisclosureAction,
  revokeContactDisclosureAction,
} from "@/lib/collaboration/actions";
import { CONTACT_DATA_TYPE, TASK_STATUS_VALUES } from "@/lib/collaboration/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";

export const metadata: Metadata = { title: "Collaboration workspace" };

export default async function CollaborationWorkspacePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const { accessToken } = await requireSession();

  const [workspaceResult, messages, notes, tasks, viewings, files, activities] = await Promise.all([
    collaborationsApi.get(accessToken, id),
    collaborationsApi.listMessages(accessToken, id),
    collaborationsApi.listNotes(accessToken, id),
    collaborationsApi.listTasks(accessToken, id),
    collaborationsApi.listViewings(accessToken, id),
    collaborationsApi.listFiles(accessToken, id),
    collaborationsApi.listActivities(accessToken, id),
  ]);

  if (!workspaceResult.ok) {
    if (workspaceResult.status === 404) notFound();
    return <p className="text-sm text-destructive">Couldn&apos;t load this workspace: {workspaceResult.error}</p>;
  }
  const workspace = workspaceResult.data;

  const activeListingDisclosure = workspace.contactDisclosures.find((d) => d.dataType === CONTACT_DATA_TYPE.ListingContact && !d.revokedAt);
  const activeDemandDisclosure = workspace.contactDisclosures.find((d) => d.dataType === CONTACT_DATA_TYPE.DemandContact && !d.revokedAt);

  // Which org owns which side isn't in this DTO (only titles are), so both disclosure rows are
  // shown to both participants — REAK.Api's GrantContactDisclosureAsync still enforces that only
  // the actual owning org's grant succeeds, regardless of what this page offers to click.
  const canManageListingDisclosure = workspace.listingId !== null;
  const canManageDemandDisclosure = workspace.demandId !== null;

  return (
    <div className="max-w-4xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">
          {workspace.listingTitle && workspace.demandTitle ? (
            <>
              {workspace.listingTitle} <span className="text-muted-foreground">×</span> {workspace.demandTitle}
            </>
          ) : (
            "Collaboration"
          )}
        </h1>
        <p className="mt-1 text-sm text-muted-foreground">
          {workspace.participants.map((p) => p.memberEntityName).join(" · ")} · opened {new Date(workspace.createdAt).toLocaleDateString()}
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <ShieldCheck className="h-4 w-4" aria-hidden="true" /> Contact disclosure
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <p className="text-xs text-muted-foreground">
            Accepting this collaboration never shares contact details by itself — each side must explicitly, and revocably, disclose it (spec §13.2).
          </p>
          {canManageListingDisclosure ? (
            <DisclosureRow
              label="Listing contact"
              disclosure={activeListingDisclosure}
              grantAction={grantContactDisclosureAction.bind(null, id, CONTACT_DATA_TYPE.ListingContact)}
              revokeAction={activeListingDisclosure ? revokeContactDisclosureAction.bind(null, id, activeListingDisclosure.id) : undefined}
            />
          ) : null}
          {canManageDemandDisclosure ? (
            <DisclosureRow
              label="Requirement contact"
              disclosure={activeDemandDisclosure}
              grantAction={grantContactDisclosureAction.bind(null, id, CONTACT_DATA_TYPE.DemandContact)}
              revokeAction={activeDemandDisclosure ? revokeContactDisclosureAction.bind(null, id, activeDemandDisclosure.id) : undefined}
            />
          ) : null}
          {!canManageListingDisclosure && !canManageDemandDisclosure ? (
            <p className="text-sm text-muted-foreground">
              This collaboration didn&apos;t originate from a match, so there&apos;s no listing/requirement contact to disclose here.
            </p>
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <MessageSquare className="h-4 w-4" aria-hidden="true" /> Messages
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-3">
            {messages.ok && messages.data.length > 0 ? (
              messages.data.map((m) => (
                <div key={m.id} className="text-sm">
                  <span className="font-medium text-foreground">{m.senderProfileName}</span>{" "}
                  <span className="text-xs text-muted-foreground">{new Date(m.createdAt).toLocaleString()}</span>
                  <p className="text-foreground">{m.body}</p>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No messages yet.</p>
            )}
          </div>
          <form action={sendMessageAction.bind(null, id)} className="flex gap-2">
            <Textarea name="body" placeholder="Write a message…" rows={2} required className="flex-1" />
            <SubmitButton size="sm">Send</SubmitButton>
          </form>
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <StickyNote className="h-4 w-4" aria-hidden="true" /> Notes
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-3">
              {notes.ok && notes.data.length > 0 ? (
                notes.data.map((n) => (
                  <div key={n.id} className="text-sm">
                    <span className="font-medium text-foreground">{n.authorProfileName}</span>{" "}
                    <span className="text-xs text-muted-foreground">{new Date(n.createdAt).toLocaleString()}</span>
                    <p className="text-foreground">{n.body}</p>
                  </div>
                ))
              ) : (
                <p className="text-sm text-muted-foreground">No internal notes yet.</p>
              )}
            </div>
            <form action={addNoteAction.bind(null, id)} className="space-y-2">
              <Textarea name="body" placeholder="Add a private note…" rows={2} required />
              <SubmitButton size="sm" variant="secondary">Add note</SubmitButton>
            </form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <ListChecks className="h-4 w-4" aria-hidden="true" /> Tasks
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              {tasks.ok && tasks.data.length > 0 ? (
                tasks.data.map((t) => (
                  <div key={t.id} className="flex items-center justify-between gap-2 text-sm">
                    <div className="min-w-0">
                      <p className={t.status === "Done" ? "truncate text-muted-foreground line-through" : "truncate text-foreground"}>{t.title}</p>
                      {t.dueDate ? <p className="text-xs text-muted-foreground">Due {new Date(t.dueDate).toLocaleDateString()}</p> : null}
                    </div>
                    {t.status !== "Done" ? (
                      <form action={updateTaskStatusAction.bind(null, id, t.id, TASK_STATUS_VALUES.Done)}>
                        <SubmitButton size="sm" variant="ghost">Mark done</SubmitButton>
                      </form>
                    ) : (
                      <Badge variant="success">Done</Badge>
                    )}
                  </div>
                ))
              ) : (
                <p className="text-sm text-muted-foreground">No tasks yet.</p>
              )}
            </div>
            <form action={createTaskAction.bind(null, id)} className="space-y-2">
              <Input name="title" placeholder="New task…" required />
              <Input name="dueDate" type="date" />
              <SubmitButton size="sm" variant="secondary">Add task</SubmitButton>
            </form>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CalendarClock className="h-4 w-4" aria-hidden="true" /> Viewings
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            {viewings.ok && viewings.data.length > 0 ? (
              viewings.data.map((v) => (
                <div key={v.id} className="text-sm">
                  <p className="font-medium text-foreground">{new Date(v.scheduledAt).toLocaleString()}</p>
                  <p className="text-xs text-muted-foreground">Scheduled by {v.scheduledByProfileName}</p>
                  {v.notes ? <p className="text-foreground">{v.notes}</p> : null}
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No viewings scheduled yet.</p>
            )}
          </div>
          <form action={scheduleViewingAction.bind(null, id)} className="space-y-2">
            <Label htmlFor="scheduledAt">Schedule a viewing</Label>
            <Input id="scheduledAt" name="scheduledAt" type="datetime-local" required />
            <Textarea name="notes" placeholder="Notes (optional)" rows={2} />
            <SubmitButton size="sm" variant="secondary">Schedule</SubmitButton>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Paperclip className="h-4 w-4" aria-hidden="true" /> Files
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            {files.ok && files.data.length > 0 ? (
              files.data.map((f) => (
                <div key={f.id} className="flex items-center justify-between gap-2 text-sm">
                  <a href={`/api/portal/collaborations/${id}/files/${f.id}/download`} className="truncate text-primary hover:underline">
                    {f.fileName}
                  </a>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted-foreground">{f.uploadedByProfileName}</span>
                    <form action={deleteFileAction.bind(null, id, f.id)}>
                      <button type="submit" className="text-muted-foreground hover:text-destructive" aria-label="Delete file">
                        <Trash2 className="h-4 w-4" aria-hidden="true" />
                      </button>
                    </form>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No files shared yet.</p>
            )}
          </div>
          <form action={uploadFileAction.bind(null, id)} className="flex items-center gap-2">
            <input type="file" name="file" required className="text-sm text-foreground" />
            <SubmitButton size="sm" variant="secondary">Upload</SubmitButton>
          </form>
        </CardContent>
      </Card>

      {activities.ok && activities.data.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <History className="h-4 w-4" aria-hidden="true" /> Activity
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {activities.data.map((a) => (
              <p key={a.id} className="text-sm text-muted-foreground">
                {a.profileName ? <span className="font-medium text-foreground">{a.profileName}: </span> : null}
                {a.action} <span className="text-xs">· {new Date(a.createdAt).toLocaleString()}</span>
              </p>
            ))}
          </CardContent>
        </Card>
      ) : null}

      <Button href="/portal/collaborations" variant="secondary" size="sm">
        Back to collaborations
      </Button>
    </div>
  );
}

function DisclosureRow({
  label,
  disclosure,
  grantAction,
  revokeAction,
}: {
  label: string;
  disclosure: { grantingMemberEntityName: string; receivingMemberEntityName: string; grantedAt: string } | undefined;
  grantAction: () => Promise<void>;
  revokeAction?: () => Promise<void>;
}) {
  return (
    <div className="flex items-center justify-between gap-4 rounded-md border border-border p-3">
      <div>
        <p className="text-sm font-medium text-foreground">{label}</p>
        {disclosure ? (
          <p className="text-xs text-muted-foreground">
            Disclosed by {disclosure.grantingMemberEntityName} to {disclosure.receivingMemberEntityName} on {new Date(disclosure.grantedAt).toLocaleDateString()}
          </p>
        ) : (
          <p className="text-xs text-muted-foreground">Not disclosed yet. Only the organization on this side can disclose it.</p>
        )}
      </div>
      {disclosure && revokeAction ? (
        <form action={revokeAction}>
          <SubmitButton size="sm" variant="destructive">Revoke</SubmitButton>
        </form>
      ) : (
        <form action={grantAction}>
          <SubmitButton size="sm">Disclose</SubmitButton>
        </form>
      )}
    </div>
  );
}
