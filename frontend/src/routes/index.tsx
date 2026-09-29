import type { RoutesProps } from "./types";
import { coreRoutes, moduleSelectionRoute } from "./modules/core.routes";
import { dashboardRoutes, erpRoutes } from "./modules/erp-legacy.routes";
import { hrmRoutes } from "./modules/hrm.routes";
import { crmRoutes } from "./modules/crm.routes";
import { posRoutes } from "./modules/pos.routes";
import { financeRoutes } from "./modules/finance.routes";
import { inventoryRoutes } from "./modules/inventory.routes";
import { procurementRoutes } from "./modules/procurement.routes";
import { manufacturingRoutes } from "./modules/manufacturing.routes";
import { projectsRoutes } from "./modules/projects.routes";
import { landingRoutes } from "./modules/landing.routes";
import { authRoutes, otherPublicRoutes } from "./modules/public.routes";
import { devRoutes } from "./modules/dev.routes";
import {
  calendarAppRoutes,
  chartRoutes,
  customPagesRoutes,
  fileAppRoutes,
  kanbanAppRoutes,
  projectAppRoutes,
  tableRoutes,
  ticketsAppRoutes,
  uiRoutes,
} from "./modules/template.routes";

export type { RoutesProps } from "./types";

/** Konrix template demo pages. Hidden unless VITE_SHOW_TEMPLATE_PAGES=true (removed in Checkpoint 12). */
export const SHOW_TEMPLATE_PAGES = import.meta.env.VITE_SHOW_TEMPLATE_PAGES === "true";

const templateRoutes: RoutesProps[] = [
  calendarAppRoutes,
  ticketsAppRoutes,
  projectAppRoutes,
  kanbanAppRoutes,
  fileAppRoutes,
  customPagesRoutes,
  tableRoutes,
  uiRoutes,
  chartRoutes,
];

const moduleRoutes: RoutesProps[] = [
  moduleSelectionRoute,
  ...landingRoutes,
  coreRoutes,
  dashboardRoutes,
  erpRoutes,
  hrmRoutes,
  crmRoutes,
  posRoutes,
  financeRoutes,
  inventoryRoutes,
  procurementRoutes,
  manufacturingRoutes,
  projectsRoutes,
];

/** Flattens nested route groups; every route already carries its absolute path. */
export function flattenRoutes(routes: RoutesProps[]): RoutesProps[] {
  return routes.flatMap((route) => [route, ...flattenRoutes(route.children ?? [])]);
}

export function getAuthProtectedRoutes(includeTemplatePages = SHOW_TEMPLATE_PAGES): RoutesProps[] {
  return includeTemplatePages ? [...moduleRoutes, ...templateRoutes] : moduleRoutes;
}

export const authProtectedRoutes = getAuthProtectedRoutes();
// The gallery needs no API, so it is public; import.meta.env.DEV removes it from production builds.
export const publicRoutes: RoutesProps[] = [...authRoutes, ...(import.meta.env.DEV ? devRoutes : []), ...otherPublicRoutes];

export const authProtectedFlattenRoutes = flattenRoutes(authProtectedRoutes);
export const publicProtectedFlattenRoutes = flattenRoutes(publicRoutes);
