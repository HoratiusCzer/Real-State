import type { Metadata } from "next";
import { createNoticeAction } from "@/lib/admin/actions";
import { Card, CardContent } from "@/components/ui/card";
import { Input, Label, Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = { title: "New notice" };

export default function NewAdminNoticePage() {
  return (
    <div className="max-w-2xl space-y-6">
      <h1 className="font-heading text-2xl font-bold text-foreground">New notice</h1>
      <Card>
        <CardContent className="space-y-4 pt-6">
          <form action={createNoticeAction} className="space-y-4">
            <div>
              <Label htmlFor="slug">Slug</Label>
              <Input id="slug" name="slug" placeholder="agm-2026" required />
            </div>
            <div>
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" required />
            </div>
            <div>
              <Label htmlFor="body">Body</Label>
              <Textarea id="body" name="body" rows={10} />
            </div>
            <div className="flex gap-2">
              <SubmitButton>Create draft</SubmitButton>
              <Button href="/admin/cms/notices" variant="secondary">Cancel</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
