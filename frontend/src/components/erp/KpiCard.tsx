import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import ReactApexChart from "react-apexcharts";
import type { ApexOptions } from "apexcharts";
import { computeDelta, formatKpiValue, type KpiFormat } from "../../lib/erp/kpi";
import { SkeletonLine } from "./Skeleton";

export interface KpiCardProps {
  /** Sentence case, no trailing colon: "Open opportunities". */
  label: string;
  value: number | null | undefined;
  format?: KpiFormat;
  currency?: string;
  /** Same measure for the comparison period; enables the delta. */
  previousValue?: number | null;
  /** Named comparison period shown with the delta: "vs last month". */
  comparisonLabel?: string;
  /** Whether an increase is good news (revenue) or bad news (overdue invoices). Default true. */
  higherIsBetter?: boolean;
  /** Up to ~12 points, oldest first; the last point is the current period. */
  trend?: number[];
  /** Drill-down page for the number. */
  to?: string;
  isLoading?: boolean;
  /** The backend has no data source for this KPI yet: show that, never a placeholder number. */
  unavailable?: boolean;
  icon?: string;
  footer?: ReactNode;
}

const SENTIMENT_CLASS = { good: "text-success", bad: "text-danger", neutral: "text-slate-500 dark:text-slate-400" } as const;
const DIRECTION_ICON = { up: "mgc_arrow_up_line", down: "mgc_arrow_down_line", flat: "mgc_minimize_line" } as const;

export function KpiCard({
  label,
  value,
  format = "number",
  currency,
  previousValue,
  comparisonLabel = "vs previous period",
  higherIsBetter = true,
  trend,
  to,
  isLoading,
  unavailable,
  icon,
  footer,
}: KpiCardProps) {
  const hasValue = typeof value === "number" && Number.isFinite(value);
  const delta = hasValue && typeof previousValue === "number" ? computeDelta(value, previousValue, higherIsBetter) : null;

  const body = (
    <div className="flex h-full flex-col gap-2 p-5">
      <div className="flex items-start justify-between gap-2">
        <h3 className="text-sm font-medium text-slate-600 dark:text-slate-300">{label}</h3>
        {icon && <i className={`${icon} text-xl text-slate-400`} aria-hidden="true" />}
      </div>

      {isLoading ? (
        <div role="status" aria-label={`Loading ${label}`} className="space-y-2">
          <SkeletonLine className="h-8 w-24" />
          <SkeletonLine className="h-3 w-32" />
        </div>
      ) : unavailable ? (
        <p className="text-sm text-slate-500 dark:text-slate-400">
          <span className="block text-2xl font-semibold text-slate-300 dark:text-slate-600" aria-hidden="true">
            —
          </span>
          Not available yet
        </p>
      ) : (
        <>
          <p className="text-2xl font-semibold text-slate-900 dark:text-slate-100">
            {hasValue ? formatKpiValue(value, format, currency) : "—"}
          </p>
          {delta && (
            <p className={`flex items-center gap-1 text-xs font-medium ${SENTIMENT_CLASS[delta.sentiment]}`}>
              <i className={DIRECTION_ICON[delta.direction]} aria-hidden="true" />
              <span>
                {delta.percent === null
                  ? delta.direction === "flat"
                    ? "No change"
                    : delta.direction === "up"
                      ? "Up from 0"
                      : "Down to 0"
                  : `${delta.percent > 0 ? "+" : ""}${new Intl.NumberFormat(undefined, { style: "percent", maximumFractionDigits: 1 }).format(delta.percent)}`}
              </span>
              <span className="font-normal text-slate-500 dark:text-slate-400">{comparisonLabel}</span>
            </p>
          )}
          {trend && trend.length > 1 && <Sparkline data={trend} label={label} format={format} currency={currency} />}
        </>
      )}
      {footer && <div className="mt-auto pt-2 text-xs text-slate-500">{footer}</div>}
    </div>
  );

  const cardClass = "card h-full transition-shadow";
  return to && !unavailable ? (
    <Link to={to} className={`${cardClass} block hover:shadow-md focus-visible:ring-2 focus-visible:ring-primary`} aria-label={`${label}: view details`}>
      {body}
    </Link>
  ) : (
    <div className={cardClass}>{body}</div>
  );
}

/** De-emphasized line with the current (last) point in the accent color; values available on hover. */
function Sparkline({ data, label, format, currency }: { data: number[]; label: string; format: KpiFormat; currency?: string }) {
  const options: ApexOptions = {
    chart: { type: "line", sparkline: { enabled: true }, animations: { enabled: false } },
    stroke: { width: 2, curve: "straight" },
    colors: ["#94a3b8"],
    markers: {
      size: 0,
      discrete: [{ seriesIndex: 0, dataPointIndex: data.length - 1, fillColor: "#3073F1", strokeColor: "#ffffff", size: 4 }],
    },
    tooltip: {
      fixed: { enabled: false },
      x: { show: false },
      y: { title: { formatter: () => "" }, formatter: (v: number) => formatKpiValue(v, format, currency) },
      marker: { show: false },
    },
  };
  return (
    <div className="mt-1" role="img" aria-label={`${label} trend over the last ${data.length} periods`}>
      <ReactApexChart type="line" height={36} series={[{ name: label, data }]} options={options} />
    </div>
  );
}
