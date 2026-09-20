"use client";

import { useActionState } from "react";
import { inviteStaffAction } from "@/lib/portal/actions";
import type { FormState } from "@/lib/auth/actions";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { FormAlert } from "@/components/auth/form-alert";

export function InviteStaffForm({ memberEntityId }: { memberEntityId: string }) {
  const boundAction = inviteStaffAction.bind(null, memberEntityId) as (
    state: FormState,
    formData: FormData
  ) => Promise<FormState>;
  const [state, formAction] = useActionState(boundAction, undefined);

  return (
    <form action={formAction} className="space-y-4" noValidate>
      <FormAlert state={state} />
      <div>
        <Label htmlFor="staff-email">Email</Label>
        <Input id="staff-email" name="email" type="email" required />
        <p className="mt-1 text-xs text-muted-foreground">
          Invites them into your organization as staff. They&apos;ll be able to browse and create listings/requirements
          for your org, but not manage its settings or invite others.
        </p>
      </div>
      <SubmitButton size="sm">Send invitation</SubmitButton>
    </form>
  );
}
