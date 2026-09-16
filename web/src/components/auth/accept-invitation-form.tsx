"use client";

import { useActionState } from "react";
import Link from "next/link";
import { acceptInvitationAction, type FormState } from "@/lib/auth/actions";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "./submit-button";
import { FormAlert } from "./form-alert";

export function AcceptInvitationForm({ token, isNewAccount }: { token: string; isNewAccount: boolean }) {
  const boundAction = acceptInvitationAction.bind(null, token) as (
    state: FormState,
    formData: FormData
  ) => Promise<FormState>;
  const [state, formAction] = useActionState(boundAction, undefined);

  if (state?.success) {
    return (
      <>
        <FormAlert state={state} />
        <Link href="/login" className="text-sm text-primary hover:underline">
          Go to login
        </Link>
      </>
    );
  }

  return (
    <form action={formAction} className="space-y-5" noValidate>
      <FormAlert state={state} />

      {isNewAccount ? (
        <>
          <div>
            <Label htmlFor="password">Choose a password</Label>
            <Input id="password" name="password" type="password" autoComplete="new-password" minLength={8} required />
          </div>
          <div>
            <Label htmlFor="confirmPassword">Confirm password</Label>
            <Input
              id="confirmPassword"
              name="confirmPassword"
              type="password"
              autoComplete="new-password"
              minLength={8}
              required
            />
          </div>
        </>
      ) : (
        <p className="text-sm text-muted-foreground">
          You already have a REAK account. Accepting will add this organization and role to your existing
          account.
        </p>
      )}

      <SubmitButton className="w-full">Accept invitation</SubmitButton>
    </form>
  );
}
