import type { Metadata } from "next";
import { createNewsAction } from "@/lib/admin/actions";
import { Card, CardContent } from "@/components/ui/card";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = { title: "New article" };

export default function NewAdminNewsPage() {
  return (
    <div className="max-w-2xl space-y-6">
      <h1 className="font-heading text-2xl font-bold text-foreground">New news article</h1>
      <Card>
        <CardContent className="space-y-4 pt-6">
          <form action={createNewsAction} className="space-y-4">
            <div>
              <Label htmlFor="slug">Slug</Label>
              <Input id="slug" name="slug" placeholder="platform-launch" required />
            </div>
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" required />
            </div>
            <div>
              <Label htmlFor="summary">Summary</Label>
              <Textarea id="summary" name="summary" rows={2} />
            </div>
            <div>
              <Label htmlFor="body">Body</Label>
              <Textarea id="body" name="body" rows={10} />
            </div>
            <div className="flex gap-2">
              <SubmitButton>Create draft</SubmitButton>
              <Button href="/admin/cms/news" variant="secondary">Cancel</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
