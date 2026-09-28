/* eslint-disable react-refresh/only-export-components */
import React from "react";
import { Navigate, Route, RouteProps } from "react-router-dom";

// components
import PrivateRoute from "./PrivateRoute";
import TicketsApp from "../pages/apps/Tickets";

// lazy load all the views

// auth
const Login = React.lazy(() => import("../pages/auth/Login"));
const Register = React.lazy(() => import("../pages/auth/Register"));
const RecoverPassword = React.lazy(() => import("../pages/auth/RecoverPassword"));
const LockScreen = React.lazy(() => import("../pages/auth/LockScreen"));

// setup
const SuperAdminSetup = React.lazy(() => import("../pages/setup/SuperAdminSetup"));
const RootRedirect = React.lazy(() => import("./RootRedirect"));

// dashboard 
const Dashboard = React.lazy(() => import("../pages/dashboard/"));

// Module Selection
const ModuleSelection = React.lazy(() => import("../pages/modules/ModuleSelection"));

// Core Settings
const CoreSettings = React.lazy(() => import("../modules/core/entities/settings/pages/Settings"));
const SystemSettingList = React.lazy(() => import("../modules/core/entities/settings/pages/SystemSettingList"));
const SystemSettingForm = React.lazy(() => import("../modules/core/entities/settings/pages/SystemSettingForm"));

// Core Users
const UserList = React.lazy(() => import("../modules/core/entities/user/pages/UserList"));
const UserForm = React.lazy(() => import("../modules/core/entities/user/pages/UserForm"));
const ResetPasswordForm = React.lazy(() => import("../modules/core/entities/user/pages/ResetPasswordForm"));

// Core Roles
const RoleList = React.lazy(() => import("../modules/core/entities/role/pages/RoleList"));
const RoleForm = React.lazy(() => import("../modules/core/entities/role/pages/RoleForm"));
const AssignPermissionsForm = React.lazy(() => import("../modules/core/entities/role/pages/AssignPermissionsForm"));

// Core Tenants
const TenantList = React.lazy(() => import("../modules/core/entities/tenant/pages/TenantList"));
const TenantForm = React.lazy(() => import("../modules/core/entities/tenant/pages/TenantForm"));

// Core Audit Logs
const AuditLogList = React.lazy(() => import("../modules/core/entities/audit-log/pages/AuditLogList"));

// Core System Logs
const SystemLogList = React.lazy(() => import("../modules/core/entities/system-log/pages/SystemLogList"));

// HRM Employees
const EmployeeList = React.lazy(() => import("../modules/hrm/entities/employee/pages/EmployeeList"));
const EmployeeForm = React.lazy(() => import("../modules/hrm/entities/employee/pages/EmployeeForm"));

// HRM Departments
const DepartmentList = React.lazy(() => import("../modules/hrm/entities/department/pages/DepartmentList"));
const DepartmentForm = React.lazy(() => import("../modules/hrm/entities/department/pages/DepartmentForm"));

// HRM Payroll
const PayrollList = React.lazy(() => import("../modules/hrm/entities/payroll/pages/PayrollList"));
const PayrollForm = React.lazy(() => import("../modules/hrm/entities/payroll/pages/PayrollForm"));

// HRM LeaveRequest
const LeaveRequestList = React.lazy(() => import("../modules/hrm/entities/leave-request/pages/LeaveRequestList"));
const LeaveRequestForm = React.lazy(() => import("../modules/hrm/entities/leave-request/pages/LeaveRequestForm"));

// HRM Training
const TrainingList = React.lazy(() => import("../modules/hrm/entities/training/pages/TrainingList"));
const TrainingForm = React.lazy(() => import("../modules/hrm/entities/training/pages/TrainingForm"));

// HRM PerformanceReview
const PerformanceReviewList = React.lazy(() => import("../modules/hrm/entities/performance-review/pages/PerformanceReviewList"));
const PerformanceReviewForm = React.lazy(() => import("../modules/hrm/entities/performance-review/pages/PerformanceReviewForm"));

// HRM TimeTracking
const TimeEntryList = React.lazy(() => import("../modules/hrm/entities/time-tracking/pages/TimeEntryList"));
const TimeEntryForm = React.lazy(() => import("../modules/hrm/entities/time-tracking/pages/TimeEntryForm"));

// CRM Customers
const CustomerList = React.lazy(() => import("../modules/crm/entities/customer/pages/CustomerList"));
const CustomerForm = React.lazy(() => import("../modules/crm/entities/customer/pages/CustomerForm"));

// CRM Contacts
const ContactList = React.lazy(() => import("../modules/crm/entities/contact/pages/ContactList"));
const ContactForm = React.lazy(() => import("../modules/crm/entities/contact/pages/ContactForm"));

// CRM Leads
const LeadsPage = React.lazy(() => import("../modules/crm/entities/lead/pages/LeadsPage"));
const LeadForm = React.lazy(() => import("../modules/crm/entities/lead/pages/LeadForm"));

// CRM Opportunities
const OpportunityList = React.lazy(() => import("../modules/crm/entities/opportunity/pages/OpportunityList"));
const OpportunityForm = React.lazy(() => import("../modules/crm/entities/opportunity/pages/OpportunityForm"));

// CRM Quotes
const QuoteList = React.lazy(() => import("../modules/crm/entities/quote/pages/QuoteList"));
const QuoteForm = React.lazy(() => import("../modules/crm/entities/quote/pages/QuoteForm"));

// CRM SalesOrders
const SalesOrderList = React.lazy(() => import("../modules/crm/entities/sales-order/pages/SalesOrderList"));
const SalesOrderForm = React.lazy(() => import("../modules/crm/entities/sales-order/pages/SalesOrderForm"));

// CRM Activities
const ActivityList = React.lazy(() => import("../modules/crm/entities/activity/pages/ActivityList"));
const ActivityForm = React.lazy(() => import("../modules/crm/entities/activity/pages/ActivityForm"));

// CRM Campaigns
const CampaignList = React.lazy(() => import("../modules/crm/entities/campaign/pages/CampaignList"));
const CampaignForm = React.lazy(() => import("../modules/crm/entities/campaign/pages/CampaignForm"));

// CRM Dashboard & extended
const CrmDashboard = React.lazy(() => import("../pages/crm/CrmDashboard"));
const CustomerGroupList = React.lazy(() => import("../modules/crm/entities/customer-group/pages/CustomerGroupList"));
const CustomerGroupForm = React.lazy(() => import("../modules/crm/entities/customer-group/pages/CustomerGroupForm"));
const PriceListList = React.lazy(() => import("../modules/crm/entities/price-list/pages/PriceListList"));
const PriceListForm = React.lazy(() => import("../modules/crm/entities/price-list/pages/PriceListForm"));
const SalesTerritoryList = React.lazy(() => import("../modules/crm/entities/sales-territory/pages/SalesTerritoryList"));
const SalesTerritoryForm = React.lazy(() => import("../modules/crm/entities/sales-territory/pages/SalesTerritoryForm"));
const ReturnOrderList = React.lazy(() => import("../modules/crm/entities/return-order/pages/ReturnOrderList"));
const ReturnOrderForm = React.lazy(() => import("../modules/crm/entities/return-order/pages/ReturnOrderForm"));

// Procurement Vendors
const VendorList = React.lazy(() => import("../modules/procurement/entities/vendor/pages/VendorList"));
const VendorForm = React.lazy(() => import("../modules/procurement/entities/vendor/pages/VendorForm"));

// Procurement PurchaseOrders
const PurchaseOrderList = React.lazy(() => import("../modules/procurement/entities/purchase-order/pages/PurchaseOrderList"));
const PurchaseOrderForm = React.lazy(() => import("../modules/procurement/entities/purchase-order/pages/PurchaseOrderForm"));

// Manufacturing ProductionOrders
const ProductionOrderList = React.lazy(() => import("../modules/manufacturing/entities/production-order/pages/ProductionOrderList"));
const ProductionOrderForm = React.lazy(() => import("../modules/manufacturing/entities/production-order/pages/ProductionOrderForm"));

// Projects
const ProjectList = React.lazy(() => import("../modules/projects/entities/project/pages/ProjectList"));
const ProjectForm = React.lazy(() => import("../modules/projects/entities/project/pages/ProjectForm"));

// POS Sessions
const POSSessionList = React.lazy(() => import("../modules/pos/entities/pos-session/pages/POSSessionList"));
const POSSessionForm = React.lazy(() => import("../modules/pos/entities/pos-session/pages/POSSessionForm"));

// POS Transactions
const POSTransactionList = React.lazy(() => import("../modules/pos/entities/pos-transaction/pages/POSTransactionList"));
const POSTransactionForm = React.lazy(() => import("../modules/pos/entities/pos-transaction/pages/POSTransactionForm"));

// Finance ChartOfAccount
const ChartOfAccountList = React.lazy(() => import("../modules/finance/entities/chart-of-account/pages/ChartOfAccountList"));
const ChartOfAccountForm = React.lazy(() => import("../modules/finance/entities/chart-of-account/pages/ChartOfAccountForm"));

// Finance Currency
const CurrencyList = React.lazy(() => import("../modules/finance/entities/currency/pages/CurrencyList"));
const CurrencyForm = React.lazy(() => import("../modules/finance/entities/currency/pages/CurrencyForm"));

// Finance Journal
const JournalList = React.lazy(() => import("../modules/finance/entities/journal/pages/JournalList"));
const JournalForm = React.lazy(() => import("../modules/finance/entities/journal/pages/JournalForm"));

// Finance Invoice
const InvoiceList = React.lazy(() => import("../modules/finance/entities/invoice/pages/InvoiceList"));
const InvoiceForm = React.lazy(() => import("../modules/finance/entities/invoice/pages/InvoiceForm"));

// Finance Bill
const BillList = React.lazy(() => import("../modules/finance/entities/bill/pages/BillList"));
const BillForm = React.lazy(() => import("../modules/finance/entities/bill/pages/BillForm"));

// Finance Payment
const PaymentList = React.lazy(() => import("../modules/finance/entities/payment/pages/PaymentList"));
const PaymentForm = React.lazy(() => import("../modules/finance/entities/payment/pages/PaymentForm"));

// Finance Bank
const BankList = React.lazy(() => import("../modules/finance/entities/bank/pages/BankList"));
const BankForm = React.lazy(() => import("../modules/finance/entities/bank/pages/BankForm"));

// Inventory ItemCategory
const ItemCategoryList = React.lazy(() => import("../modules/inventory/entities/item-category/pages/ItemCategoryList"));
const ItemCategoryForm = React.lazy(() => import("../modules/inventory/entities/item-category/pages/ItemCategoryForm"));

// Inventory Warehouse
const WarehouseList = React.lazy(() => import("../modules/inventory/entities/warehouse/pages/WarehouseList"));
const WarehouseForm = React.lazy(() => import("../modules/inventory/entities/warehouse/pages/WarehouseForm"));

// Inventory UnitOfMeasure
const UnitOfMeasureList = React.lazy(() => import("../modules/inventory/entities/unit-of-measure/pages/UnitOfMeasureList"));
const UnitOfMeasureForm = React.lazy(() => import("../modules/inventory/entities/unit-of-measure/pages/UnitOfMeasureForm"));

// Inventory Item
const ItemList = React.lazy(() => import("../modules/inventory/entities/item/pages/ItemList"));
const ItemForm = React.lazy(() => import("../modules/inventory/entities/item/pages/ItemForm"));

// Inventory StockTransfer
const StockTransferList = React.lazy(() => import("../modules/inventory/entities/stock-transfer/pages/StockTransferList"));
const StockTransferForm = React.lazy(() => import("../modules/inventory/entities/stock-transfer/pages/StockTransferForm"));

// Inventory StockAdjustment
const StockAdjustmentList = React.lazy(() => import("../modules/inventory/entities/stock-adjustment/pages/StockAdjustmentList"));
const StockAdjustmentForm = React.lazy(() => import("../modules/inventory/entities/stock-adjustment/pages/StockAdjustmentForm"));

// ERP
const ERPDashboard = React.lazy(() => import("../pages/erp/Dashboard"));
const ERPTenantList = React.lazy(() => import("../pages/erp/Tenants/List"));
const ERPTenantForm = React.lazy(() => import("../pages/erp/Tenants/Form"));

// Admin
const AdminUserList = React.lazy(() => import("../pages/erp/Admin/Users/List"));
const AdminUserForm = React.lazy(() => import("../pages/erp/Admin/Users/Form"));
const AdminResetPassword = React.lazy(() => import("../pages/erp/Admin/Users/ResetPassword"));
const AdminRoleList = React.lazy(() => import("../pages/erp/Admin/Roles/List"));
const AdminRoleForm = React.lazy(() => import("../pages/erp/Admin/Roles/Form"));
const AdminAssignPermissions = React.lazy(() => import("../pages/erp/Admin/Roles/AssignPermissions"));
const AdminSystemSettings = React.lazy(() => import("../pages/erp/Admin/SystemSettings"));
const AdminSystemSettingForm = React.lazy(() => import("../pages/erp/Admin/SystemSettings/Form"));
const AuditLogs = React.lazy(() => import("../pages/erp/Admin/AuditLogs"));
const SystemLogs = React.lazy(() => import("../pages/erp/Admin/SystemLogs"));

// apps 
const CalendarApp = React.lazy(() => import('../pages/apps/Calendar'));
const FileManagerApp = React.lazy(() => import('../pages/apps/FileManager'));
const KanbanApp = React.lazy(() => import('../pages/apps/Kanban'));
const ProjectCreate = React.lazy(() => import('../pages/apps/Project/Create'));
const ProjectDetail = React.lazy(() => import('../pages/apps/Project/Detail'));
const ProjectAppList = React.lazy(() => import('../pages/apps/Project/List'));

// extra pages 
const Starter = React.lazy(() => import('../pages/extra/Starter'));
const Timeline = React.lazy(() => import('../pages/extra/TimeLine'));
const Invoice = React.lazy(() => import('../pages/extra/Invoice'));
const Gallery = React.lazy(() => import('../pages/extra/Gallery'));
const FAQs = React.lazy(() => import('../pages/extra/FAQs'));
const Pricing = React.lazy(() => import('../pages/extra/Pricing'));

// error pages
const Maintenance = React.lazy(() => import('../pages/error/Maintenance'));
const ComingSoon = React.lazy(() => import('../pages/error/ComingSoon'));
const Error404 = React.lazy(() => import('../pages/error/Error404'));
const Error404Alt = React.lazy(() => import('../pages/error/Error404Alt'));
const Error500 = React.lazy(() => import('../pages/error/Error500'));

// base ui
const Accordions = React.lazy(() => import('../pages/ui/Accordions'));
const Alerts = React.lazy(() => import('../pages/ui/Alerts'));
const Avatars = React.lazy(() => import('../pages/ui/Avatars'));
const Buttons = React.lazy(() => import('../pages/ui/Buttons'));
const Badges = React.lazy(() => import('../pages/ui/Badges'));
const Breadcrumb = React.lazy(() => import('../pages/ui/Breadcrumb'));
const Cards = React.lazy(() => import('../pages/ui/Cards'));
const Collapse = React.lazy(() => import('../pages/ui/Collapse'));
const Dismissible = React.lazy(() => import('../pages/ui/Dismissible'));
const Dropdowns = React.lazy(() => import('../pages/ui/Dropdowns'));
const Progress = React.lazy(() => import('../pages/ui/Progress'));
const Skeleton = React.lazy(() => import('../pages/ui/Skeleton'));
const Spinners = React.lazy(() => import('../pages/ui/Spinners'));
const ListGroup = React.lazy(() => import('../pages/ui/ListGroup'));
const Ratio = React.lazy(() => import('../pages/ui/Ratio'));
const Tabs = React.lazy(() => import('../pages/ui/Tabs'));
const Modals = React.lazy(() => import('../pages/ui/Modals'));
const Offcanvas = React.lazy(() => import('../pages/ui/Offcanvas'));
const Popovers = React.lazy(() => import('../pages/ui/Popovers'));
const Tooltips = React.lazy(() => import('../pages/ui/Tooltips'));
const Typography = React.lazy(() => import('../pages/ui/Typography'));

// extended ui
const Swiper = React.lazy(() => import('../pages/extended/Swiper'));
const NestableList = React.lazy(() => import('../pages/extended/NestableList'));
const Ratings = React.lazy(() => import('../pages/extended/Ratings'));
const Animation = React.lazy(() => import('../pages/extended/Animation'));
const Player = React.lazy(() => import('../pages/extended/Player'));
const Scrollbar = React.lazy(() => import('../pages/extended/Scrollbar'));
const SweetAlert = React.lazy(() => import('../pages/extended/SweetAlert'));
const TourPage = React.lazy(() => import('../pages/extended/TourPage'));
const TippyTooltip = React.lazy(() => import('../pages/extended/TippyTooltip'));
const Lightbox = React.lazy(() => import('../pages/extended/Lightbox'));

// forms
const FormElements = React.lazy(() => import('../pages/forms/FormElements'));
const FormSelect = React.lazy(() => import('../pages/forms/Select'));
const Range = React.lazy(() => import('../pages/forms/Range'));
const Pickers = React.lazy(() => import('../pages/forms/Pickers'));
const Masks = React.lazy(() => import('../pages/forms/Masks'));
const Editor = React.lazy(() => import('../pages/forms/Editor'));
const FileUploads = React.lazy(() => import('../pages/forms/FileUploads'));
const Validation = React.lazy(() => import('../pages/forms/Validation'));
const FormLayout = React.lazy(() => import('../pages/forms/FormLayout'));

// tables
const BasicTables = React.lazy(() => import('../pages/tables/BasicTables'));
const DataTables = React.lazy(() => import('../pages/tables/DataTables'));

// icons
const MingCuteIcons = React.lazy(() => import('../pages/ui/icons/MingCuteIcons'));
const FeatherIcons = React.lazy(() => import('../pages/ui/icons/FeatherIcons'));
const MaterialSymbolIcons = React.lazy(() => import('../pages/ui/icons/MaterialSymbolIcons'));

// chart
const Chart = React.lazy(() => import('../pages/ui/Chart'));

// maps
const VectorMaps = React.lazy(() => import('../pages/ui/maps/VectorMaps'));
const GoogleMaps = React.lazy(() => import('../pages/ui/maps/GoogleMaps'));

export interface RoutesProps {
  path: RouteProps["path"];
  name?: string;
  element?: RouteProps["element"];
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  route?: any;
  exact?: boolean;
  icon?: string;
  header?: string;
  roles?: string[];
  children?: RoutesProps[];
}

// Module Selection Route
const moduleSelectionRoute: RoutesProps = {
  path: "/modules",
  name: "Module Selection",
  element: <ModuleSelection />,
  route: PrivateRoute,
};

// Core Module Routes
const coreRoutes: RoutesProps = {
  path: "/core",
  name: "Core",
  icon: "settings",
  header: "Core",
  children: [
    {
      path: "/core/settings",
      name: "Settings",
      element: <CoreSettings />,
      route: PrivateRoute,
    },
    // Users
    {
      path: "/core/settings/users",
      name: "Users",
      element: <UserList />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/users/new",
      name: "Add User",
      element: <UserForm />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/users/:id/edit",
      name: "Edit User",
      element: <UserForm />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/users/:id/reset-password",
      name: "Reset Password",
      element: <ResetPasswordForm />,
      route: PrivateRoute,
    },
    // Roles
    {
      path: "/core/settings/roles",
      name: "Roles",
      element: <RoleList />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/roles/new",
      name: "Add Role",
      element: <RoleForm />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/roles/:id/edit",
      name: "Edit Role",
      element: <RoleForm />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/roles/:id/permissions",
      name: "Assign Permissions",
      element: <AssignPermissionsForm />,
      route: PrivateRoute,
    },
    // Tenants
    {
      path: "/core/settings/tenants",
      name: "Tenants",
      element: <TenantList />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/tenants/new",
      name: "Add Tenant",
      element: <TenantForm />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/tenants/:id/edit",
      name: "Edit Tenant",
      element: <TenantForm />,
      route: PrivateRoute,
    },
    // System Settings
    {
      path: "/core/settings/system-settings",
      name: "System Settings",
      element: <SystemSettingList />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/system-settings/new",
      name: "Add System Setting",
      element: <SystemSettingForm />,
      route: PrivateRoute,
    },
    {
      path: "/core/settings/system-settings/:id/edit",
      name: "Edit System Setting",
      element: <SystemSettingForm />,
      route: PrivateRoute,
    },
    // Audit Logs
    {
      path: "/core/settings/audit-logs",
      name: "Audit Logs",
      element: <AuditLogList />,
      route: PrivateRoute,
    },
    // System Logs
    {
      path: "/core/settings/system-logs",
      name: "System Logs",
      element: <SystemLogList />,
      route: PrivateRoute,
    },
  ],
};

// dashboards
const dashboardRoutes: RoutesProps = {
  path: "/home",
  name: "Dashboards",
  icon: "home",
  header: "Navigation",
  children: [
    {
      path: "/",
      name: "Root",
      element: <Navigate to='/modules' />,
      route: PrivateRoute,
    },
    {
      path: '/dashboard',
      name: "Dashboard",
      element: <Dashboard />,
      route: PrivateRoute,
    },
  ],
};

// ERP Routes
const erpRoutes: RoutesProps = {
  path: "/erp",
  name: "ERP",
  icon: "building",
  header: "ERP",
  children: [
    {
      path: "/erp/dashboard",
      name: "ERP Dashboard",
      element: <ERPDashboard />,
      route: PrivateRoute,
    },
    {
      path: "/erp/tenants",
      name: "Tenants",
      element: <ERPTenantList />,
      route: PrivateRoute,
    },
    {
      path: "/erp/tenants/new",
      name: "Add Tenant",
      element: <ERPTenantForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/tenants/:id/edit",
      name: "Edit Tenant",
      element: <ERPTenantForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/users",
      name: "Users",
      element: <AdminUserList />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/users/new",
      name: "Add User",
      element: <AdminUserForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/users/:id/edit",
      name: "Edit User",
      element: <AdminUserForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/users/:id/reset-password",
      name: "Reset Password",
      element: <AdminResetPassword />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/roles",
      name: "Roles",
      element: <AdminRoleList />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/roles/new",
      name: "Add Role",
      element: <AdminRoleForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/roles/:id/edit",
      name: "Edit Role",
      element: <AdminRoleForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/roles/:id/permissions",
      name: "Assign Permissions",
      element: <AdminAssignPermissions />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/system-settings",
      name: "System Settings",
      element: <AdminSystemSettings />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/system-settings/new",
      name: "Add System Setting",
      element: <AdminSystemSettingForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/system-settings/:id/edit",
      name: "Edit System Setting",
      element: <AdminSystemSettingForm />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/audit-logs",
      name: "Audit Logs",
      element: <AuditLogs />,
      route: PrivateRoute,
    },
    {
      path: "/erp/admin/system-logs",
      name: "System Logs",
      element: <SystemLogs />,
      route: PrivateRoute,
    },
  ],
};

// HRM Routes
const hrmRoutes: RoutesProps = {
  path: "/hrm",
  name: "HRM",
  icon: "users",
  header: "Human Resources",
  children: [
    // Employees
    {
      path: "/hrm/employees",
      name: "Employees",
      element: <EmployeeList />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/employees/new",
      name: "Add Employee",
      element: <EmployeeForm />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/employees/:id/edit",
      name: "Edit Employee",
      element: <EmployeeForm />,
      route: PrivateRoute,
    },
    // Departments
    {
      path: "/hrm/departments",
      name: "Departments",
      element: <DepartmentList />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/departments/new",
      name: "Add Department",
      element: <DepartmentForm />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/departments/:id/edit",
      name: "Edit Department",
      element: <DepartmentForm />,
      route: PrivateRoute,
    },
    // Payrolls
    {
      path: "/hrm/payrolls",
      name: "Payrolls",
      element: <PayrollList />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/payrolls/new",
      name: "Add Payroll",
      element: <PayrollForm />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/payrolls/:id/edit",
      name: "Edit Payroll",
      element: <PayrollForm />,
      route: PrivateRoute,
    },
    // Leave Requests
    {
      path: "/hrm/leave-requests",
      name: "Leave Requests",
      element: <LeaveRequestList />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/leave-requests/new",
      name: "Add Leave Request",
      element: <LeaveRequestForm />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/leave-requests/:id/edit",
      name: "Edit Leave Request",
      element: <LeaveRequestForm />,
      route: PrivateRoute,
    },
    // Trainings
    {
      path: "/hrm/trainings",
      name: "Trainings",
      element: <TrainingList />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/trainings/new",
      name: "Add Training",
      element: <TrainingForm />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/trainings/:id/edit",
      name: "Edit Training",
      element: <TrainingForm />,
      route: PrivateRoute,
    },
    // Performance Reviews
    {
      path: "/hrm/performance-reviews",
      name: "Performance Reviews",
      element: <PerformanceReviewList />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/performance-reviews/new",
      name: "Add Performance Review",
      element: <PerformanceReviewForm />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/performance-reviews/:id/edit",
      name: "Edit Performance Review",
      element: <PerformanceReviewForm />,
      route: PrivateRoute,
    },
    // Time Entries
    {
      path: "/hrm/time-entries",
      name: "Time Entries",
      element: <TimeEntryList />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/time-entries/new",
      name: "Add Time Entry",
      element: <TimeEntryForm />,
      route: PrivateRoute,
    },
    {
      path: "/hrm/time-entries/:id/edit",
      name: "Edit Time Entry",
      element: <TimeEntryForm />,
      route: PrivateRoute,
    },
  ],
};

// CRM Routes
const crmRoutes: RoutesProps = {
  path: "/crm",
  name: "CRM",
  icon: "briefcase",
  header: "Customer Relationship Management",
  children: [
    {
      path: "/crm/dashboard",
      name: "CRM Dashboard",
      element: <CrmDashboard />,
      route: PrivateRoute,
    },
    // Customers
    {
      path: "/crm/customers",
      name: "Customers",
      element: <CustomerList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/customers/new",
      name: "Add Customer",
      element: <CustomerForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/customers/:id/edit",
      name: "Edit Customer",
      element: <CustomerForm />,
      route: PrivateRoute,
    },
    // Contacts
    {
      path: "/crm/contacts",
      name: "Contacts",
      element: <ContactList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/contacts/new",
      name: "Add Contact",
      element: <ContactForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/contacts/:id/edit",
      name: "Edit Contact",
      element: <ContactForm />,
      route: PrivateRoute,
    },
    // Leads
    {
      path: "/crm/leads",
      name: "Leads",
      element: <LeadsPage />,
      route: PrivateRoute,
    },
    {
      path: "/crm/leads/new",
      name: "Add Lead",
      element: <LeadForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/leads/:id/edit",
      name: "Edit Lead",
      element: <LeadForm />,
      route: PrivateRoute,
    },
    // Opportunities
    {
      path: "/crm/opportunities",
      name: "Opportunities",
      element: <OpportunityList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/opportunities/new",
      name: "Add Opportunity",
      element: <OpportunityForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/opportunities/:id/edit",
      name: "Edit Opportunity",
      element: <OpportunityForm />,
      route: PrivateRoute,
    },
    // Quotes
    {
      path: "/crm/quotes",
      name: "Quotes",
      element: <QuoteList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/quotes/new",
      name: "Add Quote",
      element: <QuoteForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/quotes/:id/edit",
      name: "Edit Quote",
      element: <QuoteForm />,
      route: PrivateRoute,
    },
    // Sales Orders
    {
      path: "/crm/sales-orders",
      name: "Sales Orders",
      element: <SalesOrderList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/sales-orders/new",
      name: "Add Sales Order",
      element: <SalesOrderForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/sales-orders/:id/edit",
      name: "Edit Sales Order",
      element: <SalesOrderForm />,
      route: PrivateRoute,
    },
    // Activities
    {
      path: "/crm/activities",
      name: "Activities",
      element: <ActivityList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/activities/new",
      name: "Add Activity",
      element: <ActivityForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/activities/:id/edit",
      name: "Edit Activity",
      element: <ActivityForm />,
      route: PrivateRoute,
    },
    // Campaigns
    {
      path: "/crm/campaigns",
      name: "Campaigns",
      element: <CampaignList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/campaigns/new",
      name: "Add Campaign",
      element: <CampaignForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/campaigns/:id/edit",
      name: "Edit Campaign",
      element: <CampaignForm />,
      route: PrivateRoute,
    },
    // Customer Groups
    {
      path: "/crm/customer-groups",
      name: "Customer Groups",
      element: <CustomerGroupList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/customer-groups/new",
      name: "Add Customer Group",
      element: <CustomerGroupForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/customer-groups/:id/edit",
      name: "Edit Customer Group",
      element: <CustomerGroupForm />,
      route: PrivateRoute,
    },
    // Price Lists
    {
      path: "/crm/price-lists",
      name: "Price Lists",
      element: <PriceListList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/price-lists/new",
      name: "Add Price List",
      element: <PriceListForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/price-lists/:id/edit",
      name: "Edit Price List",
      element: <PriceListForm />,
      route: PrivateRoute,
    },
    // Sales Territories
    {
      path: "/crm/sales-territories",
      name: "Sales Territories",
      element: <SalesTerritoryList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/sales-territories/new",
      name: "Add Sales Territory",
      element: <SalesTerritoryForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/sales-territories/:id/edit",
      name: "Edit Sales Territory",
      element: <SalesTerritoryForm />,
      route: PrivateRoute,
    },
    // Return Orders
    {
      path: "/crm/return-orders",
      name: "Return Orders",
      element: <ReturnOrderList />,
      route: PrivateRoute,
    },
    {
      path: "/crm/return-orders/new",
      name: "Add Return Order",
      element: <ReturnOrderForm />,
      route: PrivateRoute,
    },
    {
      path: "/crm/return-orders/:id/edit",
      name: "Edit Return Order",
      element: <ReturnOrderForm />,
      route: PrivateRoute,
    },
  ],
};

// POS Routes
const posRoutes: RoutesProps = {
  path: "/pos",
  name: "POS",
  icon: "shopping-cart",
  header: "Point of Sale",
  children: [
    // POS Sessions
    {
      path: "/pos/sessions",
      name: "POS Sessions",
      element: <POSSessionList />,
      route: PrivateRoute,
    },
    {
      path: "/pos/sessions/new",
      name: "Add POS Session",
      element: <POSSessionForm />,
      route: PrivateRoute,
    },
    {
      path: "/pos/sessions/:id/edit",
      name: "Edit POS Session",
      element: <POSSessionForm />,
      route: PrivateRoute,
    },
    // POS Transactions
    {
      path: "/pos/transactions",
      name: "POS Transactions",
      element: <POSTransactionList />,
      route: PrivateRoute,
    },
    {
      path: "/pos/transactions/new",
      name: "Add POS Transaction",
      element: <POSTransactionForm />,
      route: PrivateRoute,
    },
    {
      path: "/pos/transactions/:id/edit",
      name: "Edit POS Transaction",
      element: <POSTransactionForm />,
      route: PrivateRoute,
    },
  ],
};

// Finance Routes
const financeRoutes: RoutesProps = {
  path: "/finance",
  name: "Finance",
  icon: "dollar-sign",
  header: "Finance & Accounting",
  children: [
    // Chart of Accounts
    {
      path: "/finance/chart-of-accounts",
      name: "Chart of Accounts",
      element: <ChartOfAccountList />,
      route: PrivateRoute,
    },
    {
      path: "/finance/chart-of-accounts/new",
      name: "Add Chart of Account",
      element: <ChartOfAccountForm />,
      route: PrivateRoute,
    },
    {
      path: "/finance/chart-of-accounts/:id/edit",
      name: "Edit Chart of Account",
      element: <ChartOfAccountForm />,
      route: PrivateRoute,
    },
    // Currencies
    {
      path: "/finance/currencies",
      name: "Currencies",
      element: <CurrencyList />,
      route: PrivateRoute,
    },
    {
      path: "/finance/currencies/new",
      name: "Add Currency",
      element: <CurrencyForm />,
      route: PrivateRoute,
    },
    {
      path: "/finance/currencies/:id/edit",
      name: "Edit Currency",
      element: <CurrencyForm />,
      route: PrivateRoute,
    },
    // Journals
    {
      path: "/finance/journals",
      name: "Journals",
      element: <JournalList />,
      route: PrivateRoute,
    },
    {
      path: "/finance/journals/new",
      name: "Add Journal",
      element: <JournalForm />,
      route: PrivateRoute,
    },
    {
      path: "/finance/journals/:id/edit",
      name: "Edit Journal",
      element: <JournalForm />,
      route: PrivateRoute,
    },
    // Invoices
    {
      path: "/finance/invoices",
      name: "Invoices",
      element: <InvoiceList />,
      route: PrivateRoute,
    },
    {
      path: "/finance/invoices/new",
      name: "Add Invoice",
      element: <InvoiceForm />,
      route: PrivateRoute,
    },
    {
      path: "/finance/invoices/:id/edit",
      name: "Edit Invoice",
      element: <InvoiceForm />,
      route: PrivateRoute,
    },
    // Bills
    {
      path: "/finance/bills",
      name: "Bills",
      element: <BillList />,
      route: PrivateRoute,
    },
    {
      path: "/finance/bills/new",
      name: "Add Bill",
      element: <BillForm />,
      route: PrivateRoute,
    },
    {
      path: "/finance/bills/:id/edit",
      name: "Edit Bill",
      element: <BillForm />,
      route: PrivateRoute,
    },
    // Payments
    {
      path: "/finance/payments",
      name: "Payments",
      element: <PaymentList />,
      route: PrivateRoute,
    },
    {
      path: "/finance/payments/new",
      name: "Add Payment",
      element: <PaymentForm />,
      route: PrivateRoute,
    },
    {
      path: "/finance/payments/:id/edit",
      name: "Edit Payment",
      element: <PaymentForm />,
      route: PrivateRoute,
    },
    // Banks
    {
      path: "/finance/banks",
      name: "Banks",
      element: <BankList />,
      route: PrivateRoute,
    },
    {
      path: "/finance/banks/new",
      name: "Add Bank",
      element: <BankForm />,
      route: PrivateRoute,
    },
    {
      path: "/finance/banks/:id/edit",
      name: "Edit Bank",
      element: <BankForm />,
      route: PrivateRoute,
    },
  ],
};

// Inventory Routes
const inventoryRoutes: RoutesProps = {
  path: "/inventory",
  name: "Inventory",
  icon: "package",
  header: "Inventory Management",
  children: [
    // Item Categories
    {
      path: "/inventory/item-categories",
      name: "Item Categories",
      element: <ItemCategoryList />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/item-categories/new",
      name: "Add Item Category",
      element: <ItemCategoryForm />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/item-categories/:id/edit",
      name: "Edit Item Category",
      element: <ItemCategoryForm />,
      route: PrivateRoute,
    },
    // Warehouses
    {
      path: "/inventory/warehouses",
      name: "Warehouses",
      element: <WarehouseList />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/warehouses/new",
      name: "Add Warehouse",
      element: <WarehouseForm />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/warehouses/:id/edit",
      name: "Edit Warehouse",
      element: <WarehouseForm />,
      route: PrivateRoute,
    },
    // Units of Measure
    {
      path: "/inventory/units-of-measure",
      name: "Units of Measure",
      element: <UnitOfMeasureList />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/units-of-measure/new",
      name: "Add Unit of Measure",
      element: <UnitOfMeasureForm />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/units-of-measure/:id/edit",
      name: "Edit Unit of Measure",
      element: <UnitOfMeasureForm />,
      route: PrivateRoute,
    },
    // Items
    {
      path: "/inventory/items",
      name: "Items",
      element: <ItemList />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/items/new",
      name: "Add Item",
      element: <ItemForm />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/items/:id/edit",
      name: "Edit Item",
      element: <ItemForm />,
      route: PrivateRoute,
    },
    // Stock Transfers
    {
      path: "/inventory/stock-transfers",
      name: "Stock Transfers",
      element: <StockTransferList />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/stock-transfers/new",
      name: "Add Stock Transfer",
      element: <StockTransferForm />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/stock-transfers/:id/edit",
      name: "Edit Stock Transfer",
      element: <StockTransferForm />,
      route: PrivateRoute,
    },
    // Stock Adjustments
    {
      path: "/inventory/stock-adjustments",
      name: "Stock Adjustments",
      element: <StockAdjustmentList />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/stock-adjustments/new",
      name: "Add Stock Adjustment",
      element: <StockAdjustmentForm />,
      route: PrivateRoute,
    },
    {
      path: "/inventory/stock-adjustments/:id/edit",
      name: "Edit Stock Adjustment",
      element: <StockAdjustmentForm />,
      route: PrivateRoute,
    },
  ],
};

// Procurement Routes
const procurementRoutes: RoutesProps = {
  path: "/procurement",
  name: "Procurement",
  icon: "truck",
  header: "Procurement Management",
  children: [
    // Vendors
    {
      path: "/procurement/vendors",
      name: "Vendors",
      element: <VendorList />,
      route: PrivateRoute,
    },
    {
      path: "/procurement/vendors/new",
      name: "Add Vendor",
      element: <VendorForm />,
      route: PrivateRoute,
    },
    {
      path: "/procurement/vendors/:id/edit",
      name: "Edit Vendor",
      element: <VendorForm />,
      route: PrivateRoute,
    },
    // Purchase Orders
    {
      path: "/procurement/purchase-orders",
      name: "Purchase Orders",
      element: <PurchaseOrderList />,
      route: PrivateRoute,
    },
    {
      path: "/procurement/purchase-orders/new",
      name: "Add Purchase Order",
      element: <PurchaseOrderForm />,
      route: PrivateRoute,
    },
    {
      path: "/procurement/purchase-orders/:id/edit",
      name: "Edit Purchase Order",
      element: <PurchaseOrderForm />,
      route: PrivateRoute,
    },
  ],
};

// Manufacturing Routes
const manufacturingRoutes: RoutesProps = {
  path: "/manufacturing",
  name: "Manufacturing",
  icon: "factory",
  header: "Manufacturing Management",
  children: [
    // Production Orders
    {
      path: "/manufacturing/production-orders",
      name: "Production Orders",
      element: <ProductionOrderList />,
      route: PrivateRoute,
    },
    {
      path: "/manufacturing/production-orders/new",
      name: "Add Production Order",
      element: <ProductionOrderForm />,
      route: PrivateRoute,
    },
    {
      path: "/manufacturing/production-orders/:id/edit",
      name: "Edit Production Order",
      element: <ProductionOrderForm />,
      route: PrivateRoute,
    },
  ],
};

// Projects Routes
const projectsRoutes: RoutesProps = {
  path: "/projects",
  name: "Projects",
  icon: "briefcase",
  header: "Project Management",
  children: [
    // Projects
    {
      path: "/projects",
      name: "Projects",
      element: <ProjectList />,
      route: PrivateRoute,
    },
    {
      path: "/projects/new",
      name: "Add Project",
      element: <ProjectForm />,
      route: PrivateRoute,
    },
    {
      path: "/projects/:id/edit",
      name: "Edit Project",
      element: <ProjectForm />,
      route: PrivateRoute,
    },
  ],
};

// Apps
const calendarAppRoutes: RoutesProps = {
  path: "/apps/calendar",
  name: "Calendar",
  route: PrivateRoute,
  roles: ["Admin"],
  icon: "calendar",
  element: <CalendarApp />,
  header: "Apps",
};

const ticketsAppRoutes: RoutesProps = {
  path: "/apps/tickets",
  name: "Tickets",
  route: PrivateRoute,
  roles: ["Admin"],
  icon: "tickets",
  element: <TicketsApp />,
  header: "Apps",
};

const fileAppRoutes: RoutesProps = {
  path: "/apps/file-manager",
  name: "File Manager",
  route: PrivateRoute,
  roles: ["Admin"],
  icon: "filemanager",
  element: <FileManagerApp />,
  header: "Apps",
};

const kanbanAppRoutes: RoutesProps = {
  path: "/apps/kanban",
  name: "Kanban Board",
  route: PrivateRoute,
  roles: ["Admin"],
  icon: "kanban",
  element: <KanbanApp />,
  header: "Apps",
};

const projectAppRoutes: RoutesProps = {
  path: "/apps/project",
  name: "Project",
  route: PrivateRoute,
  roles: ["Admin"],
  icon: "project",
  children: [
    {
      path: '/apps/project/list',
      name: 'ProjectList',
      element: <ProjectAppList />,
      route: PrivateRoute,
    },
    {
      path: '/apps/project/detail',
      name: 'ProjectDetail',
      element: <ProjectDetail />,
      route: PrivateRoute,
    },
    {
      path: '/apps/project/create',
      name: 'ProjectCreate',
      element: <ProjectCreate />,
      route: PrivateRoute,
    },
  ]
};

const appRoutes = [calendarAppRoutes, ticketsAppRoutes, projectAppRoutes, kanbanAppRoutes, fileAppRoutes];

// pages
const customPagesRoutes = {
  path: "/pages",
  name: "Pages",
  icon: "pages",
  header: "Custom",
  children: [
    {
      path: "/pages/starter",
      name: "Starter",
      element: <Starter />,
      route: PrivateRoute,
    },
    {
      path: "/pages/timeline",
      name: "Timeline",
      element: <Timeline />,
      route: PrivateRoute,
    },
    {
      path: "/pages/invoice",
      name: "Invoice",
      element: <Invoice />,
      route: PrivateRoute,
    },
    {
      path: "/pages/gallery",
      name: "Gallery",
      element: <Gallery />,
      route: PrivateRoute,
    },
    {
      path: "/pages/faqs",
      name: "FAQs",
      element: <FAQs />,
      route: PrivateRoute,
    },
    {
      path: "/pages/pricing",
      name: "Pricing",
      element: <Pricing />,
      route: PrivateRoute,
    },
    {
      path: "/error-404-alt",
      name: "Error - 404-alt",
      element: <Error404Alt />,
      route: PrivateRoute,
    },
  ],
};

// tables
const tableRoutes = {
  path: "/tables",
  name: "Tables",
  icon: "table",
  header: "Elements",
  children: [
    {
      path: "/ui/tables/basic-tables",
      name: "Basic Tables",
      element: <BasicTables />,
      route: PrivateRoute,
    },
    {
      path: "/ui/tables/data-tables",
      name: "Data Tables",
      element: <DataTables />,
      route: PrivateRoute,
    },
  ]
}

// ui
const uiRoutes: RoutesProps = {
  path: "/ui",
  name: "Components",
  icon: "pocket",
  header: "Elements",
  children: [
    {
      path: "/ui/components",
      name: "Components",
      children: [
        {
          path: "/ui/accordions",
          name: "Accordions",
          element: <Accordions />,
          route: PrivateRoute,
        },
        {
          path: "/ui/alerts",
          name: "Alerts",
          element: <Alerts />,
          route: PrivateRoute,
        },
        {
          path: "/ui/avatars",
          name: "Avatars",
          element: <Avatars />,
          route: PrivateRoute,
        },
        {
          path: "/ui/buttons",
          name: "Buttons",
          element: <Buttons />,
          route: PrivateRoute,
        },
        {
          path: "/ui/badges",
          name: "Badges",
          element: <Badges />,
          route: PrivateRoute,
        },
        {
          path: "/ui/breadcrumb",
          name: "Breadcrumb",
          element: <Breadcrumb />,
          route: PrivateRoute,
        },
        {
          path: "/ui/cards",
          name: "Cards",
          element: <Cards />,
          route: PrivateRoute,
        },
        {
          path: "/ui/collapse",
          name: "Collapse",
          element: <Collapse />,
          route: PrivateRoute,
        },
        {
          path: "/ui/dismissible",
          name: "Dismissible",
          element: <Dismissible />,
          route: PrivateRoute,
        },
        {
          path: "/ui/dropdowns",
          name: "Dropdowns",
          element: <Dropdowns />,
          route: PrivateRoute,
        },
        {
          path: "/ui/progress",
          name: "Progress",
          element: <Progress />,
          route: PrivateRoute,
        },
        {
          path: "/ui/skeleton",
          name: "Skeleton",
          element: <Skeleton />,
          route: PrivateRoute,
        },
        {
          path: "/ui/spinners",
          name: "Spinners",
          element: <Spinners />,
          route: PrivateRoute,
        },
        {
          path: "/ui/list-group",
          name: "List Group",
          element: <ListGroup />,
          route: PrivateRoute,
        },
        {
          path: "/ui/ratio",
          name: "Ratio",
          element: <Ratio />,
          route: PrivateRoute,
        },
        {
          path: "/ui/tab",
          name: "Tab",
          element: <Tabs />,
          route: PrivateRoute,
        },
        {
          path: "/ui/modals",
          name: "Modals",
          element: <Modals />,
          route: PrivateRoute,
        },
        {
          path: "/ui/offcanvas",
          name: "Offcanvas",
          element: <Offcanvas />,
          route: PrivateRoute,
        },
        {
          path: "/ui/popovers",
          name: "Popovers",
          element: <Popovers />,
          route: PrivateRoute,
        },
        {
          path: "/ui/tooltips",
          name: "Tooltips",
          element: <Tooltips />,
          route: PrivateRoute,
        },
        {
          path: "/ui/typography",
          name: "Typography",
          element: <Typography />,
          route: PrivateRoute,
        },
      ],
    },
    {
      path: '/extended',
      name: 'Extended',
      children: [
        {
          path: "/extended/swiper",
          name: "Swiper",
          element: <Swiper />,
          route: PrivateRoute,
        },
        {
          path: "/extended/nestable-list",
          name: "Nestable List",
          element: <NestableList />,
          route: PrivateRoute,
        },
        {
          path: "/extended/ratings",
          name: "Ratings",
          element: <Ratings />,
          route: PrivateRoute,
        },
        {
          path: "/extended/animation",
          name: "Animation",
          element: <Animation />,
          route: PrivateRoute,
        },
        {
          path: "/extended/player",
          name: "Player",
          element: <Player />,
          route: PrivateRoute,
        },
        {
          path: "/extended/scrollbar",
          name: "Scrollbar",
          element: <Scrollbar />,
          route: PrivateRoute,
        },
        {
          path: "/extended/sweet-alert",
          name: "Sweet Alert",
          element: <SweetAlert />,
          route: PrivateRoute,
        },
        {
          path: "/extended/tour",
          name: "Tourpage",
          element: <TourPage />,
          route: PrivateRoute,
        },
        {
          path: "/extended/tooltippy",
          name: "Tippy Tooltip",
          element: <TippyTooltip />,
          route: PrivateRoute,
        },
        {
          path: "/extended/lightbox",
          name: "Lightbox",
          element: <Lightbox />,
          route: PrivateRoute,
        },
      ],
    },
    {
      path: '/ui/forms',
      name: 'Forms',
      children: [
        {
          path: "/ui/forms/form-elements",
          name: "Form Elements",
          element: <FormElements />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/select",
          name: "Select",
          element: <FormSelect />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/range",
          name: "Range",
          element: <Range />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/pickers",
          name: "Pickers",
          element: <Pickers />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/masks",
          name: "Masks",
          element: <Masks />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/editor",
          name: "Editor",
          element: <Editor />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/file-upload",
          name: "File Uploads",
          element: <FileUploads />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/validation",
          name: "Validation",
          element: <Validation />,
          route: PrivateRoute,
        },
        {
          path: "/ui/forms/form-layout",
          name: "Form Layout",
          element: <FormLayout />,
          route: PrivateRoute,
        },
      ],
    },
    {
      path: '/ui/icons',
      name: 'Icons',
      children: [
        {
          path: "/ui/icons/mingcute",
          name: "Mingcute",
          element: <MingCuteIcons />,
          route: PrivateRoute,
        },
        {
          path: "/ui/icons/feather",
          name: "Feather",
          element: <FeatherIcons />,
          route: PrivateRoute,
        },
        {
          path: "/ui/icons/material",
          name: "Material Symbols",
          element: <MaterialSymbolIcons />,
          route: PrivateRoute,
        },
      ],
    },
    {
      path: '/ui/maps',
      name: 'Maps',
      children: [
        {
          path: "/ui/maps/vector-maps",
          name: "Vector Maps",
          element: <VectorMaps />,
          route: PrivateRoute,
        },
        {
          path: "/ui/maps/google-maps",
          name: "Google Maps",
          element: <GoogleMaps />,
          route: PrivateRoute,
        },
      ],
    },
  ],
};

const chartRoutes: RoutesProps = {
  path: "/ui/chart",
  name: "Chart",
  route: PrivateRoute,
  roles: ["Admin"],
  icon: "Chart",
  element: <Chart />,
  header: "Elements",
};

// auth
const authRoutes: RoutesProps[] = [
  {
    path: "/auth/login",
    name: "Login",
    element: <Login />,
    route: Route,
  },
  {
    path: "/auth/register",
    name: "Register",
    element: <Register />,
    route: Route,
  },
  {
    path: "/auth/recover-password",
    name: "Recover Password",
    element: <RecoverPassword />,
    route: Route,
  },
  {
    path: "/auth/lock-screen",
    name: "Lock Screen",
    element: <LockScreen />,
    route: Route,
  },
];

// public routes
const otherPublicRoutes = [
  {
    path: "/",
    name: "Root",
    element: <RootRedirect />,
    route: Route,
  },
  {
    path: "/setup",
    name: "Super Admin Setup",
    element: <SuperAdminSetup />,
    route: Route,
  },
  {
    path: "*",
    name: "Error - 404",
    element: <Error404 />,
    route: Route,
  },
  {
    path: "/maintenance",
    name: "Maintenance",
    element: <Maintenance />,
    route: Route,
  },
  {
    path: "/coming-soon",
    name: "Coming Soon",
    element: <ComingSoon />,
    route: Route,
  },
  {
    path: "/error-404",
    name: "Error - 404",
    element: <Error404 />,
    route: Route,
  },
  {
    path: "/error-500",
    name: "Error - 500",
    element: <Error500 />,
    route: Route,
  },
];

// flatten the list of all nested routes
const flattenRoutes = (routes: RoutesProps[]) => {
  let flatRoutes: RoutesProps[] = [];

  routes = routes || [];
  routes.forEach((item: RoutesProps) => {
    flatRoutes.push(item);
    if (typeof item.children !== "undefined") {
      flatRoutes = [...flatRoutes, ...flattenRoutes(item.children)];
    }
  });
  return flatRoutes;
};

// All routes
const authProtectedRoutes = [
  moduleSelectionRoute,
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
  ...appRoutes,
  customPagesRoutes,
  tableRoutes,
  uiRoutes,
  chartRoutes,
];
const publicRoutes = [...authRoutes, ...otherPublicRoutes];

const authProtectedFlattenRoutes = flattenRoutes([...authProtectedRoutes]);
const publicProtectedFlattenRoutes = flattenRoutes([...publicRoutes]);
export {
  publicRoutes,
  authProtectedRoutes,
  authProtectedFlattenRoutes,
  publicProtectedFlattenRoutes,
};
