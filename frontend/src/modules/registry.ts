/**
 * The single list of ERP modules and their pages. The /modules tiles, every module's sidebar and the
 * /<module> landing pages are generated from it, and routes.test.tsx checks that every link here
 * resolves to a real page, so a menu entry can't point at a missing route again.
 */

export interface ModuleEntry {
  key: string;
  label: string;
  to: string;
  icon: string;
  description?: string;
  /** Permission code needed to see the entry (enforced from Checkpoint 2). */
  permission?: string;
  /** Only shown to SuperAdmin (platform administration). */
  superAdminOnly?: boolean;
}

export interface ModuleSection {
  title: string;
  entries: ModuleEntry[];
}

export interface ErpModule {
  id: string;
  name: string;
  description: string;
  icon: string;
  /** Where the module tile and the module name in the sidebar lead. */
  home: string;
  /** URL prefixes that belong to this module (longest match wins). */
  paths: string[];
  sections: ModuleSection[];
  /** Checkpoint that builds this module's dashboard; shown on the landing page until then. */
  dashboardCheckpoint?: string;
}

const entry = (key: string, label: string, to: string, icon: string, description?: string, permission?: string): ModuleEntry => ({
  key,
  label,
  to,
  icon,
  description,
  permission,
});

export const MODULES: ErpModule[] = [
  {
    id: "core",
    name: "Core / Settings",
    description: "Company profile, preferences, numbering, email and the audit trail.",
    icon: "mgc_settings_3_line",
    home: "/core",
    paths: ["/core"],
    sections: [
      {
        title: "Organization",
        entries: [
          entry("company", "Company profile", "/core/settings/company", "mgc_building_2_line", "Name, address, currency and time zone.", "core.company.view"),
          entry("preferences", "Preferences", "/core/settings/preferences", "mgc_translate_2_line", "Language, date format and week start.", "core.settings.view"),
          entry("numbering", "Document numbering", "/core/settings/numbering", "mgc_hashtag_line", "Prefixes and sequences for quotes, orders and more.", "core.sequences.manage"),
          entry("reference", "Reference data", "/core/settings/reference-data", "mgc_book_2_line", "Currencies, units of measure and other lists.", "core.settings.view"),
        ],
      },
      {
        title: "System",
        entries: [
          entry("security", "Security", "/core/settings/security", "mgc_shield_line", "Session timeout, password and lockout rules.", "core.settings.manage"),
          entry("email", "Email", "/core/settings/email", "mgc_mail_line", "Outgoing mail server.", "core.settings.manage"),
          entry("audit", "Audit log", "/core/settings/audit-logs", "mgc_history_line", "Who changed what, and when.", "core.audit.view"),
          entry("system-log", "System log", "/core/settings/system-logs", "mgc_warning_line", "Server errors and security events.", "core.logs.view"),
        ],
      },
      {
        title: "Platform",
        entries: [
          { ...entry("tenants", "Tenants", "/core/settings/tenants", "mgc_building_4_line", "Organizations on this installation."), superAdminOnly: true },
          { ...entry("advanced", "Advanced settings", "/core/settings/system-settings", "mgc_code_line", "Raw key/value settings."), superAdminOnly: true },
        ],
      },
    ],
  },
  {
    id: "admin",
    name: "Admin",
    description: "Users, roles and permissions.",
    icon: "mgc_user_setting_line",
    home: "/core/settings/users",
    paths: ["/core/settings/users", "/core/settings/roles", "/core/settings/admin-emails"],
    dashboardCheckpoint: "Checkpoint 2",
    sections: [
      {
        title: "Access",
        entries: [
          entry("users", "Users", "/core/settings/users", "mgc_user_3_line", "People who can sign in.", "core.users.view"),
          entry("roles", "Roles", "/core/settings/roles", "mgc_key_2_line", "Roles and their permissions.", "core.roles.view"),
          entry(
            "admin-emails",
            "Admin emails",
            "/core/settings/admin-emails",
            "mgc_mail_line",
            "Notification email directory.",
            "admin.emails.view"
          ),
        ],
      },
    ],
  },
  {
    id: "crm",
    name: "CRM",
    description: "Customers, leads, pipeline, quotes and sales orders.",
    icon: "mgc_briefcase_line",
    home: "/crm/dashboard",
    paths: ["/crm"],
    sections: [
      {
        title: "Sales",
        entries: [
          entry("dashboard", "Dashboard", "/crm/dashboard", "mgc_chart_bar_line"),
          entry("customers", "Customers", "/crm/customers", "mgc_building_2_line", "People and companies you sell to.", "crm.customers.view"),
          entry("contacts", "Contacts", "/crm/contacts", "mgc_contacts_line", undefined, "crm.contacts.view"),
          entry("leads", "Leads", "/crm/leads", "mgc_user_add_line", undefined, "crm.leads.view"),
          entry("opportunities", "Opportunities", "/crm/opportunities", "mgc_target_line", undefined, "crm.opportunities.view"),
          entry("quotes", "Quotes", "/crm/quotes", "mgc_file_line", undefined, "crm.quotes.view"),
          entry("sales-orders", "Sales orders", "/crm/sales-orders", "mgc_shopping_cart_2_line", undefined, "crm.salesOrders.view"),
        ],
      },
      {
        title: "Engagement",
        entries: [
          entry("activities", "Activities", "/crm/activities", "mgc_calendar_line", undefined, "crm.activities.view"),
          entry("campaigns", "Campaigns", "/crm/campaigns", "mgc_announcement_line", undefined, "crm.campaigns.view"),
        ],
      },
      {
        title: "Setup",
        entries: [
          entry("customer-groups", "Customer groups", "/crm/customer-groups", "mgc_group_line", undefined, "crm.customerGroups.view"),
          entry("price-lists", "Price lists", "/crm/price-lists", "mgc_price_tag_line", undefined, "crm.priceLists.view"),
          entry("territories", "Sales territories", "/crm/sales-territories", "mgc_map_pin_line", undefined, "crm.territories.view"),
          entry("returns", "Return orders", "/crm/return-orders", "mgc_back_2_line", undefined, "crm.returnOrders.view"),
        ],
      },
    ],
  },
  {
    id: "pos",
    name: "Sales / POS",
    description: "Point-of-sale sessions and transactions.",
    icon: "mgc_shopping_bag_1_line",
    home: "/pos",
    paths: ["/pos"],
    dashboardCheckpoint: "Checkpoint 4",
    sections: [
      {
        title: "Point of sale",
        entries: [
          entry("sessions", "Sessions", "/pos/sessions", "mgc_time_line", "Register shifts with opening and closing cash.", "pos.sessions.view"),
          entry("transactions", "Transactions", "/pos/transactions", "mgc_receipt_line", "Completed sales.", "pos.transactions.view"),
        ],
      },
    ],
  },
  {
    id: "inventory",
    name: "Inventory",
    description: "Items, warehouses, stock movements.",
    icon: "mgc_box_3_line",
    home: "/inventory",
    paths: ["/inventory"],
    dashboardCheckpoint: "Checkpoint 5",
    sections: [
      {
        title: "Catalog",
        entries: [
          entry("items", "Items", "/inventory/items", "mgc_box_3_line", "Products and services you stock or sell.", "inventory.items.view"),
          entry("categories", "Item categories", "/inventory/item-categories", "mgc_folder_line", undefined, "inventory.categories.view"),
          entry("uom", "Units of measure", "/inventory/units-of-measure", "mgc_ruler_line", undefined, "inventory.uom.view"),
        ],
      },
      {
        title: "Stock",
        entries: [
          entry("warehouses", "Warehouses", "/inventory/warehouses", "mgc_building_3_line", undefined, "inventory.warehouses.view"),
          entry("transfers", "Stock transfers", "/inventory/stock-transfers", "mgc_transfer_line", undefined, "inventory.transfers.view"),
          entry("adjustments", "Stock adjustments", "/inventory/stock-adjustments", "mgc_edit_4_line", undefined, "inventory.adjustments.view"),
        ],
      },
    ],
  },
  {
    id: "finance",
    name: "Finance",
    description: "Accounts, invoices, bills, payments and banking.",
    icon: "mgc_currency_dollar_line",
    home: "/finance",
    paths: ["/finance"],
    dashboardCheckpoint: "Checkpoint 6",
    sections: [
      {
        title: "Receivables & payables",
        entries: [
          entry("invoices", "Invoices", "/finance/invoices", "mgc_bill_line", "Customer invoices.", "finance.invoices.view"),
          entry("bills", "Bills", "/finance/bills", "mgc_receipt_line", "Supplier bills.", "finance.bills.view"),
          entry("payments", "Payments", "/finance/payments", "mgc_bank_card_line", undefined, "finance.payments.view"),
        ],
      },
      {
        title: "Account setup",
        entries: [
          entry("coa-setup", "Setup configuration", "/finance/accounts/setup", "mgc_settings_3_line", "Decimals, max depth, posting rules.", "finance.accounts.view"),
          entry("coa-l1", "First level", "/finance/accounts/levels/1", "mgc_list_check_line", undefined, "finance.accounts.view"),
          entry("coa-l2", "Second level", "/finance/accounts/levels/2", "mgc_list_check_line", undefined, "finance.accounts.view"),
          entry("coa-l3", "Third level", "/finance/accounts/levels/3", "mgc_list_check_line", undefined, "finance.accounts.view"),
          entry("coa-l4", "Fourth level", "/finance/accounts/levels/4", "mgc_list_check_line", undefined, "finance.accounts.view"),
          entry("coa-info", "Account information", "/finance/accounts/information", "mgc_tree_line", "Posting / leaf accounts.", "finance.accounts.view"),
          entry("coa", "Chart of accounts", "/finance/chart-of-accounts", "mgc_list_check_line", "Full CoA list.", "finance.accounts.view"),
          entry("cost-centers", "Cost centers", "/finance/cost-centers", "mgc_building_1_line", undefined, "finance.accounts.view"),
          entry("account-defaults", "Default accounts", "/finance/account-defaults", "mgc_bookmark_line", "System posting accounts (Cash, AR, AP…).", "finance.accounts.view"),
        ],
      },
      {
        title: "Vouchers",
        entries: [
          entry("cpv", "Cash payment", "/finance/vouchers/cash-payment", "mgc_wallet_2_line", undefined, "finance.journals.view"),
          entry("crv", "Cash receipt", "/finance/vouchers/cash-receipt", "mgc_wallet_3_line", undefined, "finance.journals.view"),
          entry("bpv", "Bank payment", "/finance/vouchers/bank-payment", "mgc_bank_card_line", undefined, "finance.journals.view"),
          entry("brv", "Bank receipt", "/finance/vouchers/bank-receipt", "mgc_bank_line", undefined, "finance.journals.view"),
          entry("jv", "Journal voucher", "/finance/journals/new", "mgc_book_2_line", "Manual balanced JV.", "finance.journals.view"),
          entry("approve-voucher", "Approve voucher", "/finance/vouchers/approve", "mgc_check_circle_line", "Draft vouchers awaiting approval.", "finance.journals.view"),
          entry("closed-voucher", "Closed vouchers", "/finance/vouchers/closed", "mgc_folder_check_line", "Approved vouchers (post to GL).", "finance.journals.view"),
          entry("posted-voucher", "Posted vouchers", "/finance/vouchers/posted", "mgc_file_check_line", "Posted to GL — unpost if needed.", "finance.journals.view"),
          entry("journals", "Journals", "/finance/journals", "mgc_book_3_line", undefined, "finance.journals.view"),
        ],
      },
      {
        title: "Accounts reports",
        entries: [
          entry("acct-reports", "Accounts reports", "/finance/reports", "mgc_chart_bar_line", "TB, running TB, ledger, P&L, BS, vouchers, CoA.", "finance.reports.view"),
        ],
      },
      {
        title: "Banking & FX",
        entries: [
          entry("banks", "Banks", "/finance/banks", "mgc_bank_line", undefined, "finance.banks.view"),
          entry("currencies", "Currencies", "/finance/currencies", "mgc_coin_line", undefined, "finance.currencies.view"),
        ],
      },
    ],
  },
  {
    id: "hrm",
    name: "HRM",
    description: "Employees, leave, payroll and performance.",
    icon: "mgc_user_4_line",
    home: "/hrm",
    paths: ["/hrm"],
    dashboardCheckpoint: "Checkpoint 7",
    sections: [
      {
        title: "People",
        entries: [
          entry("employees", "Employees", "/hrm/employees", "mgc_user_3_line", undefined, "hrm.employees.view"),
          entry("departments", "Departments", "/hrm/departments", "mgc_building_1_line", undefined, "hrm.departments.view"),
        ],
      },
      {
        title: "Time & pay",
        entries: [
          entry("leave", "Leave requests", "/hrm/leave-requests", "mgc_calendar_2_line", undefined, "hrm.leave.view"),
          entry("time", "Time entries", "/hrm/time-entries", "mgc_time_line", undefined, "hrm.time.view"),
          entry("payroll", "Payroll", "/hrm/payrolls", "mgc_wallet_line", undefined, "hrm.payroll.view"),
        ],
      },
      {
        title: "Development",
        entries: [
          entry("training", "Training", "/hrm/trainings", "mgc_mortarboard_line", undefined, "hrm.training.view"),
          entry("performance", "Performance reviews", "/hrm/performance-reviews", "mgc_star_line", undefined, "hrm.performance.view"),
        ],
      },
    ],
  },
  {
    id: "procurement",
    name: "Procurement",
    description: "Vendors and purchase orders.",
    icon: "mgc_truck_line",
    home: "/procurement",
    paths: ["/procurement"],
    dashboardCheckpoint: "Checkpoint 8",
    sections: [
      {
        title: "Purchasing",
        entries: [
          entry("vendors", "Vendors", "/procurement/vendors", "mgc_store_2_line", undefined, "procurement.vendors.view"),
          entry("purchase-orders", "Purchase orders", "/procurement/purchase-orders", "mgc_shopping_cart_1_line", undefined, "procurement.purchaseOrders.view"),
        ],
      },
    ],
  },
  {
    id: "manufacturing",
    name: "Manufacturing",
    description: "Production orders and work centers.",
    icon: "mgc_tool_line",
    home: "/manufacturing",
    paths: ["/manufacturing"],
    dashboardCheckpoint: "Checkpoint 9",
    sections: [
      {
        title: "Production",
        entries: [entry("production-orders", "Production orders", "/manufacturing/production-orders", "mgc_settings_6_line", undefined, "manufacturing.productionOrders.view")],
      },
    ],
  },
  {
    id: "projects",
    name: "Projects",
    description: "Projects, tasks and milestones.",
    icon: "mgc_task_2_line",
    home: "/projects",
    paths: ["/projects"],
    dashboardCheckpoint: "Checkpoint 10",
    sections: [
      {
        title: "Delivery",
        entries: [entry("projects", "Projects", "/projects", "mgc_task_2_line", undefined, "projects.projects.view")],
      },
    ],
  },
  {
    id: "reports",
    name: "Reports",
    description: "Cross-module reports and exports.",
    icon: "mgc_chart_line_line",
    home: "/reports",
    paths: ["/reports"],
    dashboardCheckpoint: "Checkpoint 11",
    sections: [],
  },
];

/** The module a URL belongs to (longest matching prefix), if any. */
export function moduleForPath(pathname: string): ErpModule | undefined {
  let best: { module: ErpModule; length: number } | undefined;
  for (const module of MODULES) {
    for (const prefix of module.paths) {
      const matches = pathname === prefix || pathname.startsWith(prefix + "/");
      if (matches && (!best || prefix.length > best.length)) best = { module, length: prefix.length };
    }
  }
  return best?.module;
}

export const findModule = (id: string) => MODULES.find((m) => m.id === id);

/** Every URL the registry links to, for the route test. */
export const registryLinks = (): string[] =>
  MODULES.flatMap((m) => [m.home, ...m.sections.flatMap((s) => s.entries.map((e) => e.to))]);
