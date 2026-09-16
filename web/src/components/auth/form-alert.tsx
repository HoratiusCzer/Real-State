import { Alert } from "@/components/ui/alert";
import type { FormState } from "@/lib/auth/actions";

export function FormAlert({ state }: { state: FormState }) {
  if (!state?.error && !state?.success) {
    return null;
  }

  return (
    <Alert variant={state.error ? "destructive" : "success"} className="mb-6">
      {state.error ?? state.success}
    </Alert>
  );
}
