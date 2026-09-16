"use client";

import { useActionState } from "react";
import Link from "next/link";
import { resetPasswordAction } from "@/lib/auth/actions";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "./submit-button";
import { FormAlert } from "./form-alert";

export function ResetPasswordForm({ token }: { token: string }) {
  const [state, formAction] = useActionState(resetPasswordAction, undefined);

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
      <input type="hidden" name="token" value={token} />

      <div>
        <Label htmlFor="newPassword">New password</Label>
        <Input id="newPassword" name="newPassword" type="password" autoComplete="new-password" minLength={8} required />
      </div>

      <div>
        <Label htmlFor="confirmPassword">Confirm new password</Label>
        <Input id="confirmPassword" name="confirmPassword" type="password" autoComplete="new-password" minLength={8} required />
      </div>

      <SubmitButton className="w-full">Reset password</SubmitButton>
    </form>
  );
}
