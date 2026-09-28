import type { ReactNode } from "react";
import { Helmet } from "react-helmet";
import { Link } from "react-router-dom";
import config from "../../config";

export interface Crumb {
  label: string;
  to?: string;
}

export interface PageHeaderProps {
  title: string;
  description?: ReactNode;
  /** Trail above the title, e.g. [{ label: "CRM", to: "/crm" }, { label: "Customers" }]. */
  breadcrumbs?: Crumb[];
  /** Right-aligned buttons; primary action last. */
  actions?: ReactNode;
  /** Extra content under the title row, such as status badges or key facts. */
  meta?: ReactNode;
}

/** Title row for every ERP page. Also sets the browser tab title. */
export function PageHeader({ title, description, breadcrumbs = [], actions, meta }: PageHeaderProps) {
  const documentTitle = [title, breadcrumbs[0]?.label, config.APP_NAME].filter(Boolean).join(" · ");

  return (
    <header className="mb-5">
      <Helmet>
        <title>{documentTitle}</title>
      </Helmet>
      {breadcrumbs.length > 0 && (
        <nav aria-label="Breadcrumb" className="mb-1">
          <ol className="flex flex-wrap items-center gap-1 text-sm text-slate-500 dark:text-slate-400">
            {breadcrumbs.map((crumb, index) => (
              <li key={`${crumb.label}-${index}`} className="flex items-center gap-1">
                {index > 0 && <i className="mgc_right_line text-slate-400 rtl:rotate-180" aria-hidden="true" />}
                {crumb.to ? (
                  <Link to={crumb.to} className="hover:text-primary focus-visible:text-primary">
                    {crumb.label}
                  </Link>
                ) : (
                  <span>{crumb.label}</span>
                )}
              </li>
            ))}
          </ol>
        </nav>
      )}
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0">
          <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{title}</h1>
          {description && <p className="mt-0.5 text-sm text-slate-500 dark:text-slate-400">{description}</p>}
        </div>
        {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
      </div>
      {meta && <div className="mt-3">{meta}</div>}
    </header>
  );
}
