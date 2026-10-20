import React from "react";
import type { RoutesProps } from "../types";

const AssignPermissionsForm = React.lazy(() => import("../../modules/core/entities/role/pages/AssignPermissionsForm"));
const AuditLogExplorer = React.lazy(() => import("../../modules/core/pages/AuditLogExplorer"));
const SettingsHub = React.lazy(() => import("../../modules/core/pages/SettingsHub"));
const CompanyProfilePage = React.lazy(() => import("../../modules/core/pages/CompanyProfilePage"));
const SettingsCategoryPage = React.lazy(() => import("../../modules/core/pages/SettingsCategoryPage"));
const NumberingPage = React.lazy(() => import("../../modules/core/pages/NumberingPage"));
const ReferenceDataPage = React.lazy(() => import("../../modules/core/pages/ReferenceDataPage"));
const ModuleSelection = React.lazy(() => import("../../pages/modules/ModuleSelection"));
const ResetPasswordForm = React.lazy(() => import("../../modules/core/entities/user/pages/ResetPasswordForm"));
const RoleForm = React.lazy(() => import("../../modules/core/entities/role/pages/RoleForm"));
const RoleList = React.lazy(() => import("../../modules/core/entities/role/pages/RoleList"));
const SystemLogExplorer = React.lazy(() => import("../../modules/core/pages/SystemLogExplorer"));
const SystemSettingForm = React.lazy(() => import("../../modules/core/entities/settings/pages/SystemSettingForm"));
const SystemSettingList = React.lazy(() => import("../../modules/core/entities/settings/pages/SystemSettingList"));
const TenantForm = React.lazy(() => import("../../modules/core/entities/tenant/pages/TenantForm"));
const TenantList = React.lazy(() => import("../../modules/core/entities/tenant/pages/TenantList"));
const UserForm = React.lazy(() => import("../../modules/core/entities/user/pages/UserForm"));
const UserList = React.lazy(() => import("../../modules/core/entities/user/pages/UserList"));
const AdminEmailList = React.lazy(() => import("../../modules/core/entities/admin-email/pages/AdminEmailList"));
const AdminEmailForm = React.lazy(() => import("../../modules/core/entities/admin-email/pages/AdminEmailForm"));

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
      element: <SettingsHub />,
    },
    // Organization and system settings (Checkpoint 1)
    {
      path: "/core/settings/company",
      name: "Company profile",
      element: <CompanyProfilePage />,
      permission: "core.company.view",
    },
    {
      path: "/core/settings/preferences",
      name: "Preferences",
      element: (
        <SettingsCategoryPage
          category="preferences"
          title="Preferences"
          description="Language, date format and first day of the week for everyone in your organization."
        />
      ),
      permission: "core.settings.view",
    },
    {
      path: "/core/settings/security",
      name: "Security",
      element: (
        <SettingsCategoryPage
          category="security"
          title="Security"
          description="Session and sign-in rules."
          notice="These values are saved now. Sign-in starts enforcing them in Checkpoint 2; until then the current built-in rules apply."
        />
      ),
      permission: "core.settings.manage",
    },
    {
      path: "/core/settings/email",
      name: "Email",
      element: (
        <SettingsCategoryPage
          category="email"
          title="Email"
          description="The mail server used to send email from the system."
          notice="These settings are saved (the password encrypted), but the system doesn't send email yet."
        />
      ),
      permission: "core.settings.manage",
    },
    {
      path: "/core/settings/numbering",
      name: "Document numbering",
      element: <NumberingPage />,
      permission: "core.sequences.manage",
    },
    {
      path: "/core/settings/reference-data",
      name: "Reference data",
      element: <ReferenceDataPage />,
      permission: "core.settings.view",
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
    {
      path: "/core/settings/admin-emails",
      name: "Admin emails",
      element: <AdminEmailList />,
      permission: "admin.emails.view",
    },
    {
      path: "/core/settings/admin-emails/new",
      name: "Add admin email",
      element: <AdminEmailForm />,
      permission: "admin.emails.create",
    },
    {
      path: "/core/settings/admin-emails/:id/edit",
      name: "Edit admin email",
      element: <AdminEmailForm />,
      permission: "admin.emails.update",
    },
    // Tenants
    {
      path: "/core/settings/tenants",
      name: "Tenants",
      element: <TenantList />,
      superAdminOnly: true,
    },
    {
      path: "/core/settings/tenants/new",
      name: "Add Tenant",
      element: <TenantForm />,
      superAdminOnly: true,
    },
    {
      path: "/core/settings/tenants/:id/edit",
      name: "Edit Tenant",
      element: <TenantForm />,
      superAdminOnly: true,
    },
    // System Settings
    {
      path: "/core/settings/system-settings",
      name: "System Settings",
      element: <SystemSettingList />,
      superAdminOnly: true,
    },
    {
      path: "/core/settings/system-settings/new",
      name: "Add System Setting",
      element: <SystemSettingForm />,
      superAdminOnly: true,
    },
    {
      path: "/core/settings/system-settings/:id/edit",
      name: "Edit System Setting",
      element: <SystemSettingForm />,
      superAdminOnly: true,
    },
    // Audit Logs
    {
      path: "/core/settings/audit-logs",
      name: "Audit Logs",
      element: <AuditLogExplorer />,
      permission: "core.audit.view",
    },
    // System Logs
    {
      path: "/core/settings/system-logs",
      name: "System Logs",
      element: <SystemLogExplorer />,
      permission: "core.logs.view",
    },
  ],
};
