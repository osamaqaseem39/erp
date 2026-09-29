import { Navigate, useLocation, useParams } from "react-router-dom";
import type { RoutesProps } from "../types";

/**
 * The /erp/* admin screens were duplicates of /core/settings/* and were removed in Checkpoint 1.
 * These redirects keep old bookmarks and links working (ids and query strings are carried over).
 */
function LegacyRedirect({ to }: { to: string }) {
  const params = useParams();
  const { search } = useLocation();
  const target = to.replace(/:(\w+)/g, (_, name: string) => encodeURIComponent(params[name] ?? ""));
  return <Navigate to={`${target}${search}`} replace />;
}

const REDIRECTS: Record<string, string> = {
  "/home": "/modules",
  "/erp": "/core",
  "/erp/dashboard": "/core",
  "/erp/tenants": "/core/settings/tenants",
  "/erp/tenants/new": "/core/settings/tenants/new",
  "/erp/tenants/:id/edit": "/core/settings/tenants/:id/edit",
  "/erp/admin/users": "/core/settings/users",
  "/erp/admin/users/new": "/core/settings/users/new",
  "/erp/admin/users/:id/edit": "/core/settings/users/:id/edit",
  "/erp/admin/users/:id/reset-password": "/core/settings/users/:id/reset-password",
  "/erp/admin/roles": "/core/settings/roles",
  "/erp/admin/roles/new": "/core/settings/roles/new",
  "/erp/admin/roles/:id/edit": "/core/settings/roles/:id/edit",
  "/erp/admin/roles/:id/permissions": "/core/settings/roles/:id/permissions",
  "/erp/admin/system-settings": "/core/settings/system-settings",
  "/erp/admin/system-settings/new": "/core/settings/system-settings/new",
  "/erp/admin/system-settings/:id/edit": "/core/settings/system-settings/:id/edit",
  "/erp/admin/audit-logs": "/core/settings/audit-logs",
  "/erp/admin/system-logs": "/core/settings/system-logs",
};

export const legacyRedirectRoutes: RoutesProps[] = Object.entries(REDIRECTS).map(([path, to]) => ({
  path,
  name: `Redirect ${path}`,
  element: <LegacyRedirect to={to} />,
}));

export const LEGACY_REDIRECTS = REDIRECTS;
