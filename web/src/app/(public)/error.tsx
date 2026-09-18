"use client";

import { useEffect } from "react";
import { AlertTriangle } from "lucide-react";
import { EmptyState } from "@/components/ui/empty-state";
import { Button } from "@/components/ui/button";

export default function PublicError({ error, retry }: { error: Error & { digest?: string }; retry: () => void }) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="mx-auto flex min-h-[50vh] max-w-md items-center justify-center px-4">
      <EmptyState
        icon={AlertTriangle}
        title="Something went wrong"
        description="This page couldn't load. Please try again — if the problem continues, check back later."
        action={
          <Button onClick={() => retry()} variant="primary" size="sm">
            Try again
          </Button>
        }
      />
    </div>
  );
}
