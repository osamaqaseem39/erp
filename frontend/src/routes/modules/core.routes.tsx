import React from "react";
import type { RoutesProps } from "../types";

const AssignPermissionsForm = React.lazy(() => import("../../modules/core/entities/role/pages/AssignPermissionsForm"));
const AuditLogList = React.lazy(() => import("../../modules/core/entities/audit-log/pages/AuditLogList"));
const CoreSettings = React.lazy(() => import("../../modules/core/entities/settings/pages/Settings"));
const ModuleSelection = React.lazy(() => import("../../pages/modules/ModuleSelection"));
const ResetPasswordForm = React.lazy(() => import("../../modules/core/entities/user/pages/ResetPasswordForm"));
const RoleForm = React.lazy(() => import("../../modules/core/entities/role/pages/RoleForm"));
const RoleList = React.lazy(() => import("../../modules/core/entities/role/pages/RoleList"));
const SystemLogList = React.lazy(() => import("../../modules/core/entities/system-log/pages/SystemLogList"));
const SystemSettingForm = React.lazy(() => import("../../modules/core/entities/settings/pages/SystemSettingForm"));
const SystemSettingList = React.lazy(() => import("../../modules/core/entities/settings/pages/SystemSettingList"));
const TenantForm = React.lazy(() => import("../../modules/core/entities/tenant/pages/TenantForm"));
const TenantList = React.lazy(() => import("../../modules/core/entities/tenant/pages/TenantList"));
const UserForm = React.lazy(() => import("../../modules/core/entities/user/pages/UserForm"));
const UserList = React.lazy(() => import("../../modules/core/entities/user/pages/UserList"));

export const moduleSelectionRoute: RoutesProps = {
  path: "/modules",
  name: "Module Selection",
  element: <ModuleSelection />,
};

export const coreRoutes: RoutesProps = {
  path: "/core",
  name: "Core",
  icon: "settings",
  header: "Core",
  children: [
    {
      path: "/core/settings",
      name: "Settings",
      element: <CoreSettings />,
    },
    // Users
    {
      path: "/core/settings/users",
      name: "Users",
      element: <UserList />,
    },
    {
      path: "/core/settings/users/new",
      name: "Add User",
      element: <UserForm />,
    },
    {
      path: "/core/settings/users/:id/edit",
      name: "Edit User",
      element: <UserForm />,
    },
    {
      path: "/core/settings/users/:id/reset-password",
      name: "Reset Password",
      element: <ResetPasswordForm />,
    },
    // Roles
    {
      path: "/core/settings/roles",
      name: "Roles",
      element: <RoleList />,
    },
    {
      path: "/core/settings/roles/new",
      name: "Add Role",
      element: <RoleForm />,
    },
    {
      path: "/core/settings/roles/:id/edit",
      name: "Edit Role",
      element: <RoleForm />,
    },
    {
      path: "/core/settings/roles/:id/permissions",
      name: "Assign Permissions",
      element: <AssignPermissionsForm />,
    },
    // Tenants
    {
      path: "/core/settings/tenants",
      name: "Tenants",
      element: <TenantList />,
    },
    {
      path: "/core/settings/tenants/new",
      name: "Add Tenant",
      element: <TenantForm />,
    },
    {
      path: "/core/settings/tenants/:id/edit",
      name: "Edit Tenant",
      element: <TenantForm />,
    },
    // System Settings
    {
      path: "/core/settings/system-settings",
      name: "System Settings",
      element: <SystemSettingList />,
    },
    {
      path: "/core/settings/system-settings/new",
      name: "Add System Setting",
      element: <SystemSettingForm />,
    },
    {
      path: "/core/settings/system-settings/:id/edit",
      name: "Edit System Setting",
      element: <SystemSettingForm />,
    },
    // Audit Logs
    {
      path: "/core/settings/audit-logs",
      name: "Audit Logs",
      element: <AuditLogList />,
    },
    // System Logs
    {
      path: "/core/settings/system-logs",
      name: "System Logs",
      element: <SystemLogList />,
    },
  ],
};
