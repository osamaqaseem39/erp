import { Helmet } from "react-helmet";
import { Link } from "react-router-dom";
import { useSelector } from "react-redux";
import type { RootState } from "../../redux/store";
import config from "../../config";
import { MODULES } from "../../modules/registry";

/** Start page after sign-in: one tile per module, generated from modules/registry.ts. */
export default function ModuleSelection() {
  const user = useSelector((state: RootState) => state.Auth.user) as { username?: string; email?: string; firstName?: string } | null;
  const name = user?.firstName || user?.username || user?.email;

  return (
    <div className="min-h-screen bg-slate-50 px-4 py-12 dark:bg-gray-900 sm:px-6 lg:px-8">
      <Helmet>
        <title>{`Modules · ${config.APP_NAME}`}</title>
      </Helmet>
      <div className="mx-auto max-w-7xl">
        <header className="mb-10 text-center">
          <h1 className="text-3xl font-bold text-slate-900 dark:text-white">{config.APP_NAME}</h1>
          {name && <p className="mt-2 text-lg text-slate-600 dark:text-slate-400">Welcome back, {name}</p>}
          <p className="mt-1 text-slate-500">Choose where you want to work.</p>
        </header>

        <ul className="grid grid-cols-1 gap-5 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {MODULES.map((module) => (
            <li key={module.id}>
              <Link
                to={module.home}
                className="flex h-full flex-col rounded-lg border border-slate-200 bg-white p-6 shadow-sm transition hover:-translate-y-0.5 hover:shadow-lg focus-visible:ring-2 focus-visible:ring-primary dark:border-slate-700 dark:bg-gray-800"
              >
                <span className="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10 text-2xl text-primary" aria-hidden="true">
                  <i className={module.icon} />
                </span>
                <span className="text-lg font-semibold text-slate-900 dark:text-white">{module.name}</span>
                <span className="mt-1 text-sm text-slate-600 dark:text-slate-400">{module.description}</span>
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
