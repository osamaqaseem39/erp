import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { EmptyState, ErrorState, KpiCard, PageHeader, SkeletonCard, WidgetBoundary } from "../../../components/erp";
import { formatDateTime } from "../../../lib/erp/format";
import { useResource } from "../../../lib/erp/http";
import { coreApi, type WidgetResult } from "../services/coreApi";

/** Core / Settings home: system health from real data. Each card fails on its own. */
export default function CoreDashboard() {
  const { data, error, isLoading, refetch } = useResource((_: null, signal) => coreApi.dashboard(signal), null);

  const header = (
    <PageHeader
      title="System health"
      description="Sign-in activity, errors and configuration for your organization."
      breadcrumbs={[{ label: "Modules", to: "/modules" }, { label: "Core / Settings" }]}
      actions={
        <Link to="/core/settings" className="btn border border-slate-300 dark:border-slate-600">
          <i className="mgc_settings_3_line me-1" aria-hidden="true" />
          All settings
        </Link>
      }
    />
  );

  if (error && !data) {
    return (
      <>
        {header}
        <div className="card">
          <ErrorState error={error} onRetry={refetch} title="Unable to load system health" />
        </div>
      </>
    );
  }

  const users = data?.users.data;
  const security = data?.security.data;
  const errors = data?.errors.data;

  return (
    <>
      {header}

      <div className="mb-5 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <WidgetBoundary name="Active users">
          <KpiCard
            label="Active users (7 days)"
            value={users?.activeLast7Days}
            previousValue={users?.activePrevious7Days}
            comparisonLabel="vs previous 7 days"
            isLoading={isLoading}
            unavailable={!!data?.users.error}
            icon="mgc_user_3_line"
            to="/core/settings/users"
            footer={users && `${users.totalUsers} users in total`}
          />
        </WidgetBoundary>
        <WidgetBoundary name="Failed sign-ins">
          <KpiCard
            label="Failed sign-ins (24h)"
            value={security?.failedSignInsLast24h}
            previousValue={security?.failedSignInsPrevious24h}
            comparisonLabel="vs previous 24h"
            higherIsBetter={false}
            isLoading={isLoading}
            unavailable={!!data?.security.error}
            icon="mgc_shield_line"
            to="/core/settings/system-logs?source=Security"
            footer={security && `${security.lockouts7Days} lockouts in the last 7 days`}
          />
        </WidgetBoundary>
        <WidgetBoundary name="Server errors">
          <KpiCard
            label="Server errors (24h)"
            value={errors?.errorsLast24h}
            previousValue={errors?.errorsPrevious24h}
            comparisonLabel="vs previous 24h"
            higherIsBetter={false}
            isLoading={isLoading}
            unavailable={!!data?.errors.error}
            icon="mgc_warning_line"
            to="/core/settings/system-logs?logLevel=Error"
          />
        </WidgetBoundary>
        <WidgetBoundary name="Locked accounts">
          <KpiCard
            label="Locked accounts"
            value={users?.lockedAccounts}
            isLoading={isLoading}
            unavailable={!!data?.users.error}
            icon="mgc_lock_line"
            to="/core/settings/users"
          />
        </WidgetBoundary>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <WidgetBoundary name="Setup checklist">
          <Panel title="Setup checklist" widget={data?.setup} isLoading={isLoading}>
            {(items) => (
              <ul className="divide-y divide-slate-100 dark:divide-slate-700">
                {items.map((item) => (
                  <li key={item.key} className="flex items-center gap-3 px-5 py-3 text-sm">
                    <i
                      className={item.done ? "mgc_check_circle_fill text-lg text-success" : "mgc_round_line text-lg text-slate-400"}
                      aria-hidden="true"
                    />
                    <span className="flex-1">
                      {item.label}
                      <span className="sr-only">{item.done ? " (done)" : " (to do)"}</span>
                    </span>
                    {!item.done && (
                      <Link to={item.link} className="whitespace-nowrap text-primary hover:underline">
                        Set up
                      </Link>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </Panel>
        </WidgetBoundary>

        <WidgetBoundary name="Latest errors">
          <Panel title="Latest server errors" widget={data?.errors} isLoading={isLoading} viewAll="/core/settings/system-logs?logLevel=Error">
            {(w) =>
              w.latest.length === 0 ? (
                <EmptyState compact title="No server errors" description="Errors the server hits while handling requests appear here." />
              ) : (
                <ul className="divide-y divide-slate-100 dark:divide-slate-700">
                  {w.latest.map((e) => (
                    <li key={e.id} className="px-5 py-3 text-sm">
                      <p className="mb-0.5 truncate font-medium text-slate-800 dark:text-slate-100">{e.message}</p>
                      <time className="text-xs text-slate-500">{formatDateTime(e.timestamp)}</time>
                    </li>
                  ))}
                </ul>
              )
            }
          </Panel>
        </WidgetBoundary>

        <WidgetBoundary name="Configuration changes">
          <Panel title="Recent configuration changes" widget={data?.configChanges} isLoading={isLoading} viewAll="/core/settings/audit-logs">
            {(changes) =>
              changes.length === 0 ? (
                <EmptyState compact title="No changes yet" description="Changes to company, settings and numbering are recorded here." />
              ) : (
                <ul className="divide-y divide-slate-100 dark:divide-slate-700">
                  {changes.map((c) => (
                    <li key={c.id} className="flex items-baseline justify-between gap-3 px-5 py-3 text-sm">
                      <span>
                        <span className="font-medium">{labelForEntity(c.entityName)}</span> {c.action.toLowerCase()}
                        {c.userName && <span className="text-slate-500"> by {c.userName}</span>}
                      </span>
                      <time className="whitespace-nowrap text-xs text-slate-500">{formatDateTime(c.timestamp)}</time>
                    </li>
                  ))}
                </ul>
              )
            }
          </Panel>
        </WidgetBoundary>

        {data?.tenants && (
          <WidgetBoundary name="Tenants">
            <Panel title="Tenants (platform)" widget={data.tenants} isLoading={isLoading} viewAll="/core/settings/tenants">
              {(rows) => (
                <table className="min-w-full text-sm">
                  <caption className="sr-only">Tenants by status</caption>
                  <tbody>
                    {rows.map((r) => (
                      <tr key={r.status} className="border-t border-slate-100 dark:border-slate-700">
                        <th scope="row" className="px-5 py-2 text-left font-normal normal-case tracking-normal text-slate-700 dark:text-slate-200">
                          {r.status}
                        </th>
                        <td className="px-5 py-2 text-right tabular-nums">{r.count}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </Panel>
          </WidgetBoundary>
        )}
      </div>
    </>
  );
}

const ENTITY_LABELS: Record<string, string> = { SystemSetting: "Setting", Company: "Company profile", DocumentSequence: "Numbering" };
const labelForEntity = (name: string) => ENTITY_LABELS[name] ?? name;

function Panel<T>({
  title,
  widget,
  isLoading,
  viewAll,
  children,
}: {
  title: string;
  widget: WidgetResult<T> | undefined | null;
  isLoading: boolean;
  viewAll?: string;
  children: (data: T) => ReactNode;
}) {
  if (isLoading || !widget) return <SkeletonCard lines={4} />;
  return (
    <section className="card h-full" aria-label={title}>
      <div className="flex items-center justify-between border-b border-slate-100 px-5 py-3 dark:border-slate-700">
        <h2 className="text-sm font-semibold text-slate-800 dark:text-slate-100">{title}</h2>
        {viewAll && (
          <Link to={viewAll} className="text-sm text-primary hover:underline">
            View all
          </Link>
        )}
      </div>
      {widget.error || widget.data == null ? (
        <EmptyState variant="error" compact title={`${title} couldn't be loaded`} description={widget.error ?? "No data was returned."} />
      ) : (
        children(widget.data)
      )}
    </section>
  );
}
