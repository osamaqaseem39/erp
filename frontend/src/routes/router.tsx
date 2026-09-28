import { createBrowserRouter, type RouteObject } from "react-router-dom";
import App from "../App";
import DefaultLayout from "../layouts/Default";
import { RouteErrorBoundary } from "../components/erp/RouteErrorBoundary";
import { authProtectedFlattenRoutes, publicProtectedFlattenRoutes } from ".";
import { ProtectedRoute } from "./ProtectedRoute";
import type { RoutesProps } from "./types";

// Group nodes (e.g. "/hrm" with children but no element) only organize the tree; they are not pages.
const isPage = (route: RoutesProps): route is RoutesProps & { path: string } =>
  typeof route.path === "string" && route.element !== undefined;

export function buildRouteObjects(
  publicRoutes: RoutesProps[] = publicProtectedFlattenRoutes,
  protectedRoutes: RoutesProps[] = authProtectedFlattenRoutes
): RouteObject[] {
  return [
    {
      element: <App />,
      errorElement: <RouteErrorBoundary />,
      children: [
        ...publicRoutes.filter(isPage).map((route) => ({
          path: route.path,
          element: <DefaultLayout>{route.element}</DefaultLayout>,
        })),
        ...protectedRoutes.filter(isPage).map((route) => ({
          path: route.path,
          element: <ProtectedRoute route={route} />,
        })),
      ],
    },
  ];
}

export const router = createBrowserRouter(buildRouteObjects(), {
  basename: process.env.PUBLIC_URL,
  future: { v7_relativeSplatPath: true },
});
