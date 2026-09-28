const MONTH_LABELS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

export function getRecentMonthBuckets(count = 6): { key: string; label: string; year: number; month: number }[] {
  const buckets: { key: string; label: string; year: number; month: number }[] = [];
  const now = new Date();
  for (let i = count - 1; i >= 0; i--) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
    buckets.push({
      key: `${d.getFullYear()}-${d.getMonth()}`,
      label: MONTH_LABELS[d.getMonth()],
      year: d.getFullYear(),
      month: d.getMonth(),
    });
  }
  return buckets;
}

export function countRecordsByMonth(
  items: Array<{ createdAt?: string }>,
  bucketCount = 6
): { labels: string[]; counts: number[] } {
  const buckets = getRecentMonthBuckets(bucketCount);
  const counts = buckets.map(() => 0);

  items.forEach((item) => {
    if (!item.createdAt) return;
    const d = new Date(item.createdAt);
    if (Number.isNaN(d.getTime())) return;
    const idx = buckets.findIndex((b) => b.year === d.getFullYear() && b.month === d.getMonth());
    if (idx >= 0) counts[idx] += 1;
  });

  return { labels: buckets.map((b) => b.label), counts };
}

export function groupCountByField<T extends Record<string, unknown>>(
  items: T[],
  field: keyof T,
  fallback = "Unknown"
): { labels: string[]; series: number[] } {
  const map = new Map<string, number>();
  items.forEach((item) => {
    const raw = item[field];
    const key = raw != null && String(raw).trim() ? String(raw) : fallback;
    map.set(key, (map.get(key) ?? 0) + 1);
  });
  const entries = [...map.entries()].sort((a, b) => b[1] - a[1]).slice(0, 8);
  return {
    labels: entries.map(([label]) => label),
    series: entries.map(([, count]) => count),
  };
}

export function sumNumericField<T extends Record<string, unknown>>(
  items: T[],
  fields: Array<keyof T>
): number {
  return items.reduce((sum, item) => {
    for (const field of fields) {
      const value = item[field];
      if (typeof value === "number" && !Number.isNaN(value)) {
        return sum + value;
      }
    }
    return sum;
  }, 0);
}

export function calculateWinRate(
  opportunities: Array<{ status?: string; stage?: string }>
): number {
  if (opportunities.length === 0) return 0;
  const won = opportunities.filter((o) => {
    const status = (o.status ?? "").toLowerCase();
    const stage = (o.stage ?? "").toLowerCase();
    return status.includes("won") || stage.includes("won");
  }).length;
  return Math.round((won / opportunities.length) * 100);
}

export function sparklineFromCounts(counts: number[]): number[] {
  if (counts.length === 0) return [0, 0, 0, 0, 0, 0];
  return counts;
}
