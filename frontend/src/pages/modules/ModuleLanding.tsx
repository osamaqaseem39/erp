import { Link } from "react-router-dom";
import { EmptyState, PageHeader } from "../../components/erp";
import { readPermissionContext, hasPermission } from "../../lib/erp/permissions";
import { APICore } from "../../helpers/api/apiCore";
import { findModule } from "../../modules/registry";

/**
 * /<module> overview until the module's own dashboard checkpoint: what's in the module and where to go.
 * Deliberately shows no figures; a dashboard with real numbers replaces the notice later.
 */
export default function ModuleLanding({ moduleId }: { moduleId: string }) {
  const module = findModule(moduleId);
  if (!module) return <EmptyState variant="no-results" title="Unknown module" />;

  const permissions = readPermissionContext(new APICore().getLoggedInUser());
  const sections = module.sections
    .map((s) => ({
      ...s,
      entries: s.entries.filter((e) => (!e.superAdminOnly || permissions.isSuperAdmin) && (!e.permission || hasPermission(permissions, e.permission))),
    }))
    .filter((s) => s.entries.length > 0);

  return (
    <>
      <PageHeader title={module.name} description={module.description} breadcrumbs={[{ label: "Modules", to: "/modules" }, { label: module.name }]} />

      {module.dashboardCheckpoint && (
        <div className="mb-5 flex items-start gap-3 rounded-lg border border-info/30 bg-info/5 px-4 py-3 text-sm text-slate-700 dark:text-slate-200" role="note">
          <i className="mgc_time_line mt-0.5 text-lg text-info" aria-hidden="true" />
          <p className="mb-0">
            The {module.name} dashboard is built in {module.dashboardCheckpoint}. Until then, use the areas below.
          </p>
        </div>
      )}

      {sections.length === 0 ? (
        <div className="card">
          <EmptyState variant="unavailable" title={`${module.name} isn't available yet`} description={`This module arrives in ${module.dashboardCheckpoint ?? "a later checkpoint"}.`} />
        </div>
      ) : (
        <div className="space-y-6">
          {sections.map((section) => (
            <section key={section.title} aria-labelledby={`section-${section.title}`}>
              <h2 id={`section-${section.title}`} className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">
                {section.title}
              </h2>
              <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {section.entries.map((e) => (
                  <li key={e.key}>
                    <Link
                      to={e.to}
                      className="card flex h-full items-start gap-3 p-4 transition-shadow hover:shadow-md focus-visible:ring-2 focus-visible:ring-primary"
                    >
                      <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-xl text-primary" aria-hidden="true">
                        <i className={e.icon} />
                      </span>
                      <span>
                        <span className="block font-medium text-slate-900 dark:text-slate-100">{e.label}</span>
                        {e.description && <span className="mt-0.5 block text-sm text-slate-500 dark:text-slate-400">{e.description}</span>}
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            </section>
          ))}
        </div>
      )}
    </>
  );
}
