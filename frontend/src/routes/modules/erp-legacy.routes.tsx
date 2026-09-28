import React from "react";
import type { RoutesProps } from "../types";

const AdminAssignPermissions = React.lazy(() => import("../../pages/erp/Admin/Roles/AssignPermissions"));
const AdminResetPassword = React.lazy(() => import("../../pages/erp/Admin/Users/ResetPassword"));
const AdminRoleForm = React.lazy(() => import("../../pages/erp/Admin/Roles/Form"));
const AdminRoleList = React.lazy(() => import("../../pages/erp/Admin/Roles/List"));
const AdminSystemSettingForm = React.lazy(() => import("../../pages/erp/Admin/SystemSettings/Form"));
const AdminSystemSettings = React.lazy(() => import("../../pages/erp/Admin/SystemSettings"));
const AdminUserForm = React.lazy(() => import("../../pages/erp/Admin/Users/Form"));
const AdminUserList = React.lazy(() => import("../../pages/erp/Admin/Users/List"));
const AuditLogs = React.lazy(() => import("../../pages/erp/Admin/AuditLogs"));
const Dashboard = React.lazy(() => import("../../pages/dashboard/"));
const ERPDashboard = React.lazy(() => import("../../pages/erp/Dashboard"));
const ERPTenantForm = React.lazy(() => import("../../pages/erp/Tenants/Form"));
const ERPTenantList = React.lazy(() => import("../../pages/erp/Tenants/List"));
const SystemLogs = React.lazy(() => import("../../pages/erp/Admin/SystemLogs"));

export const dashboardRoutes: RoutesProps = {
  path: "/home",
  name: "Dashboards",
  icon: "home",
  header: "Navigation",
  children: [
    {
      path: '/dashboard',
      name: "Dashboard",
      element: <Dashboard />,
    },
  ],
};

export const erpRoutes: RoutesProps = {
  path: "/erp",
  name: "ERP",
  icon: "building",
  header: "ERP",
  children: [
    {
      path: "/erp/dashboard",
      name: "ERP Dashboard",
      element: <ERPDashboard />,
    },
    {
      path: "/erp/tenants",
      name: "Tenants",
      element: <ERPTenantList />,
    },
    {
      path: "/erp/tenants/new",
      name: "Add Tenant",
      element: <ERPTenantForm />,
    },
    {
      path: "/erp/tenants/:id/edit",
      name: "Edit Tenant",
      element: <ERPTenantForm />,
    },
    {
      path: "/erp/admin/users",
      name: "Users",
      element: <AdminUserList />,
    },
    {
      path: "/erp/admin/users/new",
      name: "Add User",
      element: <AdminUserForm />,
    },
    {
      path: "/erp/admin/users/:id/edit",
      name: "Edit User",
      element: <AdminUserForm />,
    },
    {
      path: "/erp/admin/users/:id/reset-password",
      name: "Reset Password",
      element: <AdminResetPassword />,
    },
    {
      path: "/erp/admin/roles",
      name: "Roles",
      element: <AdminRoleList />,
    },
    {
      path: "/erp/admin/roles/new",
      name: "Add Role",
      element: <AdminRoleForm />,
    },
    {
      path: "/erp/admin/roles/:id/edit",
      name: "Edit Role",
      element: <AdminRoleForm />,
    },
    {
      path: "/erp/admin/roles/:id/permissions",
      name: "Assign Permissions",
      element: <AdminAssignPermissions />,
    },
    {
      path: "/erp/admin/system-settings",
      name: "System Settings",
      element: <AdminSystemSettings />,
    },
    {
      path: "/erp/admin/system-settings/new",
      name: "Add System Setting",
      element: <AdminSystemSettingForm />,
    },
    {
      path: "/erp/admin/system-settings/:id/edit",
      name: "Edit System Setting",
      element: <AdminSystemSettingForm />,
    },
    {
      path: "/erp/admin/audit-logs",
      name: "Audit Logs",
      element: <AuditLogs />,
    },
    {
      path: "/erp/admin/system-logs",
      name: "System Logs",
      element: <SystemLogs />,
    },
  ],
};
