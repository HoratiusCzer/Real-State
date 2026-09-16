"use client";

import { useActionState } from "react";
import { submitMembershipApplicationAction } from "@/lib/auth/actions";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "./submit-button";
import { FormAlert } from "./form-alert";

export function MembershipApplicationForm() {
  const [state, formAction] = useActionState(submitMembershipApplicationAction, undefined);

  if (state?.success) {
    return <FormAlert state={state} />;
  }

  return (
    <form action={formAction} className="space-y-5" noValidate>
      <FormAlert state={state} />

      <div>
        <Label htmlFor="companyName">Company name</Label>
        <Input id="companyName" name="companyName" required />
      </div>

      <div>
        <Label htmlFor="contactName">Contact person</Label>
        <Input id="contactName" name="contactName" required />
      </div>

      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
        <div>
          <Label htmlFor="email">Email</Label>
          <Input id="email" name="email" type="email" required />
        </div>
        <div>
          <Label htmlFor="phone">Phone</Label>
          <Input id="phone" name="phone" type="tel" required />
        </div>
      </div>

      <div>
        <Label htmlFor="message">Message (optional)</Label>
        <textarea
          id="message"
          name="message"
          rows={4}
          className="flex w-full rounded-md border border-border bg-card px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
        />
      </div>

      <SubmitButton>Submit application</SubmitButton>
    </form>
  );
}
