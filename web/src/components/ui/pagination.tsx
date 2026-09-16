import { Button } from "@/components/ui/button";

export function Pagination({
  basePath,
  page,
  pageSize,
  totalCount,
  searchParams,
}: {
  basePath: string;
  page: number;
  pageSize: number;
  totalCount: number;
  searchParams: Record<string, string | undefined>;
}) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  if (totalPages <= 1) return null;

  const hrefFor = (p: number) => {
    const usp = new URLSearchParams();
    for (const [key, value] of Object.entries(searchParams)) {
      if (value && key !== "page") usp.set(key, value);
    }
    usp.set("page", String(p));
    return `${basePath}?${usp.toString()}`;
  };

  return (
    <nav className="flex items-center justify-between pt-4" aria-label="Pagination">
      <p className="text-sm text-muted-foreground">
        Page {page} of {totalPages} ({totalCount} results)
      </p>
      <div className="flex gap-2">
        {page > 1 ? (
          <Button href={hrefFor(page - 1)} variant="secondary" size="sm">
            Previous
          </Button>
        ) : null}
        {page < totalPages ? (
          <Button href={hrefFor(page + 1)} variant="secondary" size="sm">
            Next
          </Button>
        ) : null}
      </div>
    </nav>
  );
}
