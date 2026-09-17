import type { Metadata } from "next";
import { Navigation } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsNavigationApi } from "@/lib/admin/api";
import { createNavigationItemAction, toggleNavigationItemActiveAction, deleteNavigationItemAction } from "@/lib/admin/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Navigation" };

export default async function AdminNavigationPage() {
  const { accessToken } = await requireSession();
  const result = await cmsNavigationApi.list(accessToken);
  const items = result.ok ? result.data : [];

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Navigation</h1>
        <p className="mt-1 text-sm text-muted-foreground">Admin-configurable site navigation (spec §4.3).</p>
      </div>

      <Card>
        <CardHeader><CardTitle>Add a navigation item</CardTitle></CardHeader>
        <CardContent>
          <form action={createNavigationItemAction} className="space-y-4">
            <div>
              <Label htmlFor="label">Label</Label>
              <Input id="label" name="label" required />
            </div>
            <div>
              <Label htmlFor="url">URL</Label>
              <Input id="url" name="url" required placeholder="/resources" />
            </div>
            <div>
              <Label htmlFor="parentId">Parent item (optional)</Label>
              <select id="parentId" name="parentId" className="flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground">
                <option value="">None (top level)</option>
                {items.map((i) => (
                  <option key={i.id} value={i.id}>{i.label}</option>
                ))}
              </select>
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
        <p className="text-sm text-destructive">Couldn&apos;t load navigation: {result.error}</p>
      ) : items.length === 0 ? (
        <EmptyState icon={Navigation} title="No navigation items yet" description="Add the first item above." />
      ) : (
        <ul className="space-y-2">
          {items.map((i) => (
            <li key={i.id}>
              <Card className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">
                    {i.parentId ? <span className="text-muted-foreground">↳ </span> : null}
                    {i.label}
                  </p>
                  <p className="text-xs text-muted-foreground">{i.url}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant={i.isActive ? "success" : "default"}>{i.isActive ? "Active" : "Hidden"}</Badge>
                  <form action={toggleNavigationItemActiveAction.bind(null, i.id)}>
                    <SubmitButton size="sm" variant="secondary">{i.isActive ? "Hide" : "Show"}</SubmitButton>
                  </form>
                  <form action={deleteNavigationItemAction.bind(null, i.id)}>
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
