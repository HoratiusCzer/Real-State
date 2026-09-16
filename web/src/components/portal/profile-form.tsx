"use client";

import { useActionState } from "react";
import { updateProfileAction } from "@/lib/portal/actions";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { FormAlert } from "@/components/auth/form-alert";

export function ProfileForm({ fullName, phone }: { fullName: string; phone: string | null }) {
  const [state, formAction] = useActionState(updateProfileAction, undefined);

  return (
    <form action={formAction} className="space-y-5" noValidate>
      <FormAlert state={state} />

      <div>
        <Label htmlFor="fullName">Full name</Label>
        <Input id="fullName" name="fullName" defaultValue={fullName} required />
      </div>

      <div>
        <Label htmlFor="phone">Phone</Label>
        <Input id="phone" name="phone" type="tel" defaultValue={phone ?? ""} />
      </div>

      <SubmitButton>Save changes</SubmitButton>
    </form>
  );
}
