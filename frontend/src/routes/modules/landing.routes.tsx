import React from "react";
import { MODULES } from "../../modules/registry";
import type { RoutesProps } from "../types";

const ModuleLanding = React.lazy(() => import("../../pages/modules/ModuleLanding"));
const CoreDashboard = React.lazy(() => import("../../modules/core/pages/CoreDashboard"));

/** Pages that exist at a module's home URL for their own reasons (a dashboard or a single list). */
const CUSTOM_HOMES = new Set(["/core", "/projects", "/crm/dashboard", "/core/settings/users"]);

/**
 * /<module> overview pages, generated from the module registry, plus the Core dashboard at /core.
 * Before Checkpoint 1 these URLs were empty group nodes or linked from /modules to routes that didn't exist.
 */
export const landingRoutes: RoutesProps[] = [
  { path: "/core", name: "System health", element: <CoreDashboard />, permission: "core.dashboard.view" },
  ...MODULES.filter((m) => !CUSTOM_HOMES.has(m.home)).map<RoutesProps>((m) => ({
    path: m.home,
    name: m.name,
    element: <ModuleLanding moduleId={m.id} />,
  })),
];
