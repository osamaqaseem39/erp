import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@headlessui/react";
import type { ReactNode } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { formatDateTime } from "../../lib/erp/format";
import { StatusBadge } from "./StatusBadge";
import { EmptyState } from "./EmptyState";

// ---------- header ----------

export interface DetailHeaderProps {
  title: string;
  /** Business identifier users recognize (C-001, INV-2026-0042). Never a GUID. */
  code?: string;
  status?: string | null;
  statusLabels?: Record<string, string>;
  /** Key facts under the title: owner, dates, amounts. */
  facts?: { label: string; value: ReactNode }[];
  /** Primary actions (Edit, Approve…). Put destructive actions in `more`. */
  actions?: ReactNode;
  more?: ReactNode;
}

export function DetailHeader({ title, code, status, statusLabels, facts, actions, more }: DetailHeaderProps) {
  return (
    <div className="card mb-4 p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <h2 className="truncate text-lg font-semibold text-slate-900 dark:text-slate-100">{title}</h2>
            {status !== undefined && <StatusBadge status={status} labels={statusLabels} />}
          </div>
          {code && <p className="mt-0.5 text-sm text-slate-500 dark:text-slate-400">{code}</p>}
        </div>
        {(actions || more) && (
          <div className="flex flex-wrap items-center gap-2">
            {actions}
            {more}
          </div>
        )}
      </div>
      {facts && facts.length > 0 && (
        <dl className="mt-4 grid grid-cols-2 gap-x-6 gap-y-3 border-t border-slate-100 pt-4 text-sm sm:grid-cols-4 dark:border-slate-700">
          {facts.map((fact) => (
            <div key={fact.label} className="min-w-0">
              <dt className="text-xs text-slate-500 dark:text-slate-400">{fact.label}</dt>
              <dd className="mt-0.5 truncate font-medium text-slate-800 dark:text-slate-100">{fact.value}</dd>
            </div>
          ))}
        </dl>
      )}
    </div>
  );
}

// ---------- tabs ----------

export interface DetailTab {
  key: string;
  label: string;
  /** Count badge, e.g. number of contacts. */
  count?: number;
  content: ReactNode;
}

/** Tabs whose selection lives in ?tab=, so links and the back button land on the right tab. */
export function DetailTabs({ tabs }: { tabs: DetailTab[] }) {
  const [params, setParams] = useSearchParams();
  const requested = params.get("tab");
  const selectedIndex = Math.max(0, tabs.findIndex((t) => t.key === requested));

  return (
    <TabGroup
      selectedIndex={selectedIndex}
      onChange={(index) => {
        const next = new URLSearchParams(params);
        if (index === 0) next.delete("tab");
        else next.set("tab", tabs[index].key);
        setParams(next, { replace: true });
      }}
    >
      <TabList className="flex gap-1 overflow-x-auto border-b border-slate-200 dark:border-slate-700">
        {tabs.map((tab) => (
          <Tab
            key={tab.key}
            className="-mb-px whitespace-nowrap border-b-2 border-transparent px-3 py-2 text-sm font-medium text-slate-600 hover:text-slate-900 focus:outline-none data-[focus]:ring-2 data-[focus]:ring-primary data-[selected]:border-primary data-[selected]:text-primary dark:text-slate-300"
          >
            {tab.label}
            {tab.count !== undefined && (
              <span className="ms-1.5 rounded-full bg-slate-100 px-1.5 text-xs text-slate-600 dark:bg-slate-700 dark:text-slate-300">{tab.count}</span>
            )}
          </Tab>
        ))}
      </TabList>
      <TabPanels className="pt-4">
        {tabs.map((tab) => (
          <TabPanel key={tab.key} unmount>
            {tab.content}
          </TabPanel>
        ))}
      </TabPanels>
    </TabGroup>
  );
}

// ---------- fields ----------

export interface FieldItem {
  label: string;
  value: ReactNode;
  /** Span the full row (addresses, notes). */
  wide?: boolean;
}

/** Read-only label/value grid. Empty values show a dash so layout stays stable. */
export function FieldGrid({ title, fields, columns = 2 }: { title?: string; fields: FieldItem[]; columns?: 2 | 3 }) {
  return (
    <section className="card p-5">
      {title && <h3 className="mb-4 text-sm font-semibold text-slate-800 dark:text-slate-100">{title}</h3>}
      <dl className={`grid grid-cols-1 gap-x-6 gap-y-4 sm:grid-cols-2 ${columns === 3 ? "lg:grid-cols-3" : ""}`}>
        {fields.map((field) => (
          <div key={field.label} className={`min-w-0 ${field.wide ? "sm:col-span-full" : ""}`}>
            <dt className="text-xs text-slate-500 dark:text-slate-400">{field.label}</dt>
            <dd className="mt-0.5 break-words text-sm text-slate-800 dark:text-slate-100">
              {field.value === null || field.value === undefined || field.value === "" ? "—" : field.value}
            </dd>
          </div>
        ))}
      </dl>
    </section>
  );
}

// ---------- related records ----------

export function RelatedRecordsPanel({
  title,
  count,
  viewAllTo,
  action,
  children,
}: {
  title: string;
  count?: number;
  viewAllTo?: string;
  action?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="card">
      <div className="flex items-center justify-between gap-2 border-b border-slate-100 px-5 py-3 dark:border-slate-700">
        <h3 className="text-sm font-semibold text-slate-800 dark:text-slate-100">
          {title}
          {count !== undefined && <span className="ms-1 font-normal text-slate-500">({count})</span>}
        </h3>
        <div className="flex items-center gap-3 text-sm">
          {action}
          {viewAllTo && (
            <Link to={viewAllTo} className="text-primary hover:underline">
              View all
            </Link>
          )}
        </div>
      </div>
      {children}
    </section>
  );
}

// ---------- activity ----------

export interface ActivityChange {
  field: string;
  before?: ReactNode;
  after?: ReactNode;
}

export interface ActivityItem {
  id: string;
  at: string | Date;
  actor?: string;
  /** "Created", "Status changed", "Approved"… */
  title: string;
  description?: ReactNode;
  changes?: ActivityChange[];
}

export function ActivityTimeline({ items, emptyText = "No activity recorded yet." }: { items: ActivityItem[]; emptyText?: string }) {
  if (items.length === 0) return <EmptyState compact title="No activity" description={emptyText} />;
  return (
    <ol className="relative ms-3 border-s border-slate-200 dark:border-slate-700">
      {items.map((item) => (
        <li key={item.id} className="mb-5 ms-5">
          <span className="absolute -start-1.5 mt-1.5 h-3 w-3 rounded-full border-2 border-white bg-primary dark:border-gray-800" aria-hidden="true" />
          <p className="text-sm">
            <span className="font-medium text-slate-900 dark:text-slate-100">{item.title}</span>
            {item.actor && <span className="text-slate-500"> by {item.actor}</span>}
          </p>
          <time className="text-xs text-slate-500" dateTime={new Date(item.at).toISOString()}>
            {formatDateTime(item.at)}
          </time>
          {item.description && <div className="mt-1 text-sm text-slate-600 dark:text-slate-300">{item.description}</div>}
          {item.changes && item.changes.length > 0 && (
            <dl className="mt-2 space-y-1 rounded-md bg-slate-50 p-3 text-xs dark:bg-slate-800">
              {item.changes.map((change) => (
                <div key={change.field} className="flex flex-wrap gap-1">
                  <dt className="font-medium text-slate-700 dark:text-slate-200">{change.field}:</dt>
                  <dd className="text-slate-600 dark:text-slate-300">
                    <span className="line-through opacity-70">{change.before ?? "—"}</span>
                    <span aria-hidden="true"> → </span>
                    <span className="sr-only"> changed to </span>
                    <span>{change.after ?? "—"}</span>
                  </dd>
                </div>
              ))}
            </dl>
          )}
        </li>
      ))}
    </ol>
  );
}
