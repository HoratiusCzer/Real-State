"use client";

import { useEffect } from "react";
import { AlertTriangle } from "lucide-react";
import { EmptyState } from "@/components/ui/empty-state";
import { Button } from "@/components/ui/button";

export default function AdminError({ error, retry }: { error: Error & { digest?: string }; retry: () => void }) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <EmptyState
      icon={AlertTriangle}
      title="Something went wrong"
      description="This page couldn't load. Try again, or head back to the admin dashboard."
      action={
        <div className="flex justify-center gap-2">
          <Button onClick={() => retry()} variant="primary" size="sm">
            Try again
          </Button>
          <Button href="/admin" variant="secondary" size="sm">
            Admin Dashboard
          </Button>
        </div>
      }
    />
  );
}
