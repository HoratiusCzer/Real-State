import { Skeleton } from "@/components/ui/skeleton";

// A generic per-segment fallback (spec §25 "skeleton states") shown briefly while any public page
// under this route group streams in — deliberately shaped to look reasonable for both a listing
// grid and a single article/form page, since loading.tsx can't know which destination it's for.
export default function PublicLoading() {
  return (
    <section className="mx-auto max-w-4xl space-y-4 px-4 py-16 sm:px-6 lg:px-8">
      <Skeleton className="h-8 w-64" />
      <Skeleton className="h-4 w-full max-w-lg" />
      <div className="space-y-3 pt-4">
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
      </div>
    </section>
  );
}
