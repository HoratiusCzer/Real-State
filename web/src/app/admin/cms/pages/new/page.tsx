import type { Metadata } from "next";
import { createPageAction } from "@/lib/admin/actions";
import { Card, CardContent } from "@/components/ui/card";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = { title: "New page" };

export default function NewAdminPage() {
  return (
    <div className="max-w-2xl space-y-6">
      <h1 className="font-heading text-2xl font-bold text-foreground">New page</h1>
      <Card>
        <CardContent className="space-y-4 pt-6">
          <form action={createPageAction} className="space-y-4">
            <div>
              <Label htmlFor="slug">Slug</Label>
              <Input id="slug" name="slug" placeholder="about" required />
              <p className="mt-1 text-xs text-muted-foreground">Determines the public URL, e.g. &quot;about&quot; → /about.</p>
            </div>
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" required />
            </div>
            <div>
              <Label htmlFor="body">Body</Label>
              <Textarea id="body" name="body" rows={10} />
            </div>
            <div>
              <Label htmlFor="seoTitle">SEO title</Label>
              <Input id="seoTitle" name="seoTitle" />
            </div>
            <div>
              <Label htmlFor="seoDescription">SEO description</Label>
              <Textarea id="seoDescription" name="seoDescription" rows={2} />
            </div>
            <div>
              <Label htmlFor="ogImageUrl">OG image URL</Label>
              <Input id="ogImageUrl" name="ogImageUrl" type="url" />
            </div>
            <div className="flex gap-2">
              <SubmitButton>Create draft</SubmitButton>
              <Button href="/admin/cms/pages" variant="secondary">Cancel</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
