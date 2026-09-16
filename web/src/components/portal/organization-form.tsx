"use client";

import { useActionState } from "react";
import { updateOrganizationAction } from "@/lib/portal/actions";
import type { FormState } from "@/lib/auth/actions";
import type { MemberEntityDetail } from "@/lib/portal/api";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { FormAlert } from "@/components/auth/form-alert";

export function OrganizationForm({ entity }: { entity: MemberEntityDetail }) {
  const boundAction = updateOrganizationAction.bind(null, entity.id) as (
    state: FormState,
    formData: FormData
  ) => Promise<FormState>;
  const [state, formAction] = useActionState(boundAction, undefined);

  return (
    <form action={formAction} className="space-y-5" noValidate>
      <FormAlert state={state} />

      <div>
        <Label htmlFor="name">Organization name</Label>
        <Input id="name" name="name" defaultValue={entity.name} required />
      </div>

      <div>
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          name="description"
          rows={3}
          defaultValue={entity.description ?? ""}
          className="flex w-full rounded-md border border-border bg-card px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
        />
      </div>

      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
        <div>
          <Label htmlFor="website">Website</Label>
          <Input id="website" name="website" type="url" defaultValue={entity.website ?? ""} />
        </div>
        <div>
          <Label htmlFor="phone">Phone</Label>
          <Input id="phone" name="phone" type="tel" defaultValue={entity.phone ?? ""} />
        </div>
        <div>
          <Label htmlFor="email">Email</Label>
          <Input id="email" name="email" type="email" defaultValue={entity.email ?? ""} />
        </div>
        <div>
          <Label htmlFor="address">Address</Label>
          <Input id="address" name="address" defaultValue={entity.address ?? ""} />
        </div>
      </div>

      <SubmitButton>Save organization</SubmitButton>
    </form>
  );
}
