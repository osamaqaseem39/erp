import { Navigate, useLocation } from "react-router-dom";
import { APICore } from "../helpers/api/apiCore";
import DefaultLayout from "../layouts/Default";
import VerticalLayout from "../layouts/Vertical";
import { PermissionGuard } from "../components/erp/PermissionGuard";
import type { RoutesProps } from "./types";

/** Signed-in check, layout choice and route-level permission check for every authenticated page. */
export function ProtectedRoute({ route }: { route: RoutesProps }) {
  const location = useLocation();
  const api = new APICore();

  if (!api.isUserAuthenticated()) {
    api.setLoggedInUser(null);
    const next = encodeURIComponent(location.pathname + location.search);
    return <Navigate to={`/auth/login?next=${next}`} replace />;
  }

  const Layout = route.path === "/modules" ? DefaultLayout : VerticalLayout;
  const content = route.permission ? (
    <PermissionGuard permission={route.permission} fallback="page">
      {route.element}
    </PermissionGuard>
  ) : (
    route.element
  );
  return <Layout>{content}</Layout>;
}
