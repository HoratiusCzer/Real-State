"use client";

import { useActionState } from "react";
import { forgotPasswordAction } from "@/lib/auth/actions";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "./submit-button";
import { FormAlert } from "./form-alert";

export function ForgotPasswordForm() {
  const [state, formAction] = useActionState(forgotPasswordAction, undefined);

  if (state?.success) {
    return <FormAlert state={state} />;
  }

  return (
    <form action={formAction} className="space-y-5" noValidate>
      <FormAlert state={state} />

      <div>
        <Label htmlFor="email">Email</Label>
        <Input id="email" name="email" type="email" autoComplete="email" required />
      </div>

      <SubmitButton className="w-full">Send reset link</SubmitButton>
    </form>
  );
}
