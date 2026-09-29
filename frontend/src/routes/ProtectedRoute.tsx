import { useEffect } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { loadTenantContext } from "../modules/core/tenantContext";
import { APICore } from "../helpers/api/apiCore";
import DefaultLayout from "../layouts/Default";
import VerticalLayout from "../layouts/Vertical";
import { PermissionGuard } from "../components/erp/PermissionGuard";
import { PermissionDenied } from "../components/erp/PermissionDenied";
import { readPermissionContext } from "../lib/erp/permissions";
import type { RoutesProps } from "./types";

/** Signed-in check, layout choice and route-level permission check for every authenticated page. */
export function ProtectedRoute({ route }: { route: RoutesProps }) {
  const location = useLocation();
  const api = new APICore();
  const signedIn = api.isUserAuthenticated();

  // Currency, locale and time zone for every formatter (cached for the session after the first load).
  useEffect(() => {
    if (signedIn) void loadTenantContext();
  }, [signedIn]);

  if (!signedIn) {
    api.setLoggedInUser(null);
    const next = encodeURIComponent(location.pathname + location.search);
    return <Navigate to={`/auth/login?next=${next}`} replace />;
  }

  const Layout = route.path === "/modules" ? DefaultLayout : VerticalLayout;
  if (route.superAdminOnly && !readPermissionContext(api.getLoggedInUser()).isSuperAdmin) {
    return (
      <Layout>
        <PermissionDenied description="Only platform administrators can open this page." />
      </Layout>
    );
  }
  const content = route.permission ? (
    <PermissionGuard permission={route.permission} fallback="page">
      {route.element}
    </PermissionGuard>
  ) : (
    route.element
  );
  return <Layout>{content}</Layout>;
}
