import { useMemo } from "react";
import { APICore } from "../../helpers/api/apiCore";

/**
 * Permission codes follow `<module>.<resource>.<action>`, e.g. "hrm.employees.create".
 *
 * Checkpoint 0 behaviour (matches the backend today, which only requires an authenticated user):
 * - SuperAdmin role: everything allowed.
 * - Session carries a `permissions` array: only listed codes are allowed ("module.resource.*" wildcards ok).
 * - Session has no `permissions` array yet: any signed-in user is allowed.
 * Checkpoint 2 adds `permissions` to the login response, which switches every check to the strict rule
 * without changing any call site. The backend remains the security boundary either way.
 */

export const SUPER_ADMIN_ROLE = "SuperAdmin";

interface SessionShape {
  token?: string;
  roleNames?: string[];
  permissions?: string[];
}

export interface PermissionContext {
  isAuthenticated: boolean;
  isSuperAdmin: boolean;
  /** undefined = the backend has not provided permissions yet (pre-Checkpoint 2). */
  permissions: ReadonlySet<string> | undefined;
}

export function readPermissionContext(session: SessionShape | null | undefined): PermissionContext {
  const roles = session?.roleNames ?? [];
  return {
    isAuthenticated: !!session?.token,
    isSuperAdmin: roles.some((r) => r.toLowerCase() === SUPER_ADMIN_ROLE.toLowerCase()),
    permissions: Array.isArray(session?.permissions)
      ? new Set(session.permissions.map((p) => p.toLowerCase()))
      : undefined,
  };
}

export function hasPermission(ctx: PermissionContext, code: string): boolean {
  if (!ctx.isAuthenticated) return false;
  if (ctx.isSuperAdmin) return true;
  if (!ctx.permissions) return true;

  const wanted = code.toLowerCase();
  if (ctx.permissions.has(wanted)) return true;
  // "crm.customers.*" grants every action on the resource, "crm.*" every resource in the module.
  const parts = wanted.split(".");
  for (let i = parts.length - 1; i > 0; i--) {
    if (ctx.permissions.has(`${parts.slice(0, i).join(".")}.*`)) return true;
  }
  return false;
}

function useSessionPermissions(): PermissionContext {
  const session = new APICore().getLoggedInUser() as SessionShape | null;
  const token = session?.token;
  const rolesKey = (session?.roleNames ?? []).join(",");
  const permsKey = (session?.permissions ?? []).join(",");
  // The session only changes on login/refresh, which re-renders the app; key on its content.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  return useMemo(() => readPermissionContext(session), [token, rolesKey, permsKey]);
}

export function usePermission(code: string): boolean {
  const ctx = useSessionPermissions();
  return hasPermission(ctx, code);
}

export function useAnyPermission(codes: readonly string[]): boolean {
  const ctx = useSessionPermissions();
  return codes.some((code) => hasPermission(ctx, code));
}
