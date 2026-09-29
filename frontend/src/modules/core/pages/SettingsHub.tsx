import { Link } from "react-router-dom";
import { PageHeader } from "../../../components/erp";
import { APICore } from "../../../helpers/api/apiCore";
import { hasPermission, readPermissionContext } from "../../../lib/erp/permissions";
import { findModule } from "../../registry";

/** All settings in one place, grouped as in the Core sidebar (generated from the module registry). */
export default function SettingsHub() {
  const core = findModule("core")!;
  const admin = findModule("admin")!;
  const permissions = readPermissionContext(new APICore().getLoggedInUser());
  const sections = [...core.sections, ...admin.sections]
    .map((s) => ({
      ...s,
      entries: s.entries.filter((e) => (!e.superAdminOnly || permissions.isSuperAdmin) && (!e.permission || hasPermission(permissions, e.permission))),
    }))
    .filter((s) => s.entries.length > 0);

  return (
    <>
      <PageHeader title="Settings" description="Configure your organization." breadcrumbs={[{ label: "Core / Settings", to: "/core" }, { label: "Settings" }]} />
      <div className="space-y-6">
        {sections.map((section) => (
          <section key={section.title} aria-labelledby={`settings-${section.title}`}>
            <h2 id={`settings-${section.title}`} className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">
              {section.title}
            </h2>
            <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
              {section.entries.map((e) => (
                <li key={e.key}>
                  <Link to={e.to} className="card flex h-full items-start gap-3 p-4 transition-shadow hover:shadow-md focus-visible:ring-2 focus-visible:ring-primary">
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
    </>
  );
}
