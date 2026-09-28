import type { RouteProps } from "react-router-dom";

export interface RoutesProps {
  path: RouteProps["path"];
  name?: string;
  element?: RouteProps["element"];
  icon?: string;
  header?: string;
  roles?: string[];
  /** Permission code required to open the route, e.g. "crm.customers.view" (enforced from Checkpoint 2). */
  permission?: string;
  children?: RoutesProps[];
}
