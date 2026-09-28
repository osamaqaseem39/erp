const bar = "animate-pulse rounded bg-slate-200 dark:bg-slate-700";

/** A single placeholder line. */
export function SkeletonLine({ className = "h-4 w-full" }: { className?: string }) {
  return <div className={`${bar} ${className}`} aria-hidden="true" />;
}

/** Placeholder rows shaped like a table while the first page loads. */
export function SkeletonTable({ rows = 6, columns = 5 }: { rows?: number; columns?: number }) {
  return (
    <div role="status" aria-label="Loading" className="space-y-3 p-4">
      {Array.from({ length: rows }, (_, r) => (
        <div key={r} className="grid gap-4" style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}>
          {Array.from({ length: columns }, (_, c) => (
            <SkeletonLine key={c} className={`h-4 ${c === 0 ? "w-3/4" : "w-full"}`} />
          ))}
        </div>
      ))}
      <span className="sr-only">Loading…</span>
    </div>
  );
}

export function SkeletonCard({ lines = 3 }: { lines?: number }) {
  return (
    <div role="status" aria-label="Loading" className="card p-5 space-y-3">
      <SkeletonLine className="h-4 w-1/3" />
      {Array.from({ length: lines }, (_, i) => (
        <SkeletonLine key={i} className={`h-3 ${i === lines - 1 ? "w-2/3" : "w-full"}`} />
      ))}
      <span className="sr-only">Loading…</span>
    </div>
  );
}

/** Label/value pairs, for detail pages and forms being loaded for edit. */
export function SkeletonFields({ fields = 6 }: { fields?: number }) {
  return (
    <div role="status" aria-label="Loading" className="grid gap-5 sm:grid-cols-2">
      {Array.from({ length: fields }, (_, i) => (
        <div key={i} className="space-y-2">
          <SkeletonLine className="h-3 w-24" />
          <SkeletonLine className="h-9 w-full" />
        </div>
      ))}
      <span className="sr-only">Loading…</span>
    </div>
  );
}
