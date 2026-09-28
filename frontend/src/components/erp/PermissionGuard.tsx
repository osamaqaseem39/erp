import type { ReactNode } from "react";
import { useAnyPermission } from "../../lib/erp/permissions";
import { PermissionDenied } from "./PermissionDenied";

export interface PermissionGuardProps {
  /** One code, or several where any one is enough. */
  permission: string | readonly string[];
  children: ReactNode;
  /**
   * What to render without permission: "hide" (default, for buttons and widgets),
   * "page" (full permission-denied state), or a custom node.
   */
  fallback?: "hide" | "page" | ReactNode;
}

/** UX only: the API enforces permissions independently. */
export function PermissionGuard({ permission, children, fallback = "hide" }: PermissionGuardProps) {
  const codes = typeof permission === "string" ? [permission] : permission;
  const allowed = useAnyPermission(codes);
  if (allowed) return <>{children}</>;
  if (fallback === "hide") return null;
  if (fallback === "page") return <PermissionDenied />;
  return <>{fallback}</>;
}
