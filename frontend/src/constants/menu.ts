export interface MenuItemTypes {
  key: string;
  label: string;
  isTitle?: boolean;
  icon?: string;
  url?: string;
  parentKey?: string;
  target?: string;
  children?: MenuItemTypes[];
}

const MODULES_LINK: MenuItemTypes = {
  key: 'modules',
  label: 'Modules',
  isTitle: false,
  icon: 'mgc_apps_line',
  url: '/modules',
};

const HOME_MENU: MenuItemTypes[] = [
  {
    key: 'menu',
    label: 'Menu',
    isTitle: true,
  },
  {
    key: 'dashboard',
    label: 'Dashboard',
    isTitle: false,
    icon: 'mgc_home_3_line',
    url: '/dashboard',
  },
  MODULES_LINK,
];

const ERP_MENU: MenuItemTypes[] = [
  {
    key: 'erp',
    label: 'ERP',
    isTitle: true,
  },
  {
    key: 'erp-dashboard',
    label: 'ERP Dashboard',
    isTitle: false,
    icon: 'mgc_home_3_line',
    url: '/erp/dashboard',
  },
  {
    key: 'erp-tenants',
    label: 'Tenants',
    isTitle: false,
    icon: 'mgc_building_2_line',
    children: [
      {
        key: 'tenants-list',
        label: 'List',
        url: '/erp/tenants',
        parentKey: 'erp-tenants',
      },
      {
        key: 'tenants-create',
        label: 'Create',
        url: '/erp/tenants/new',
        parentKey: 'erp-tenants',
      },
    ],
  },
  {
    key: 'admin',
    label: 'Admin',
    isTitle: true,
  },
  {
    key: 'admin-users',
    label: 'Users',
    isTitle: false,
    icon: 'mgc_user_3_line',
    children: [
      {
        key: 'users-list',
        label: 'List',
        url: '/erp/admin/users',
        parentKey: 'admin-users',
      },
      {
        key: 'users-create',
        label: 'Create',
        url: '/erp/admin/users/new',
        parentKey: 'admin-users',
      },
    ],
  },
  {
    key: 'admin-roles',
    label: 'Roles',
    isTitle: false,
    icon: 'mgc_user_group_line',
    children: [
      {
        key: 'roles-list',
        label: 'List',
        url: '/erp/admin/roles',
        parentKey: 'admin-roles',
      },
      {
        key: 'roles-create',
        label: 'Create',
        url: '/erp/admin/roles/new',
        parentKey: 'admin-roles',
      },
    ],
  },
  {
    key: 'admin-settings',
    label: 'System Settings',
    isTitle: false,
    icon: 'mgc_settings_3_line',
    url: '/erp/admin/system-settings',
  },
  {
    key: 'admin-audit-logs',
    label: 'Audit Logs',
    isTitle: false,
    icon: 'mgc_file_list_line',
    url: '/erp/admin/audit-logs',
  },
  {
    key: 'admin-system-logs',
    label: 'System Logs',
    isTitle: false,
    icon: 'mgc_file_text_line',
    url: '/erp/admin/system-logs',
  },
];

const CRM_MENU: MenuItemTypes[] = [
  {
    key: 'crm',
    label: 'Sales',
    isTitle: true,
  },
  {
    key: 'crm-dashboard',
    label: 'Dashboard',
    isTitle: false,
    icon: 'mgc_chart_bar_line',
    url: '/crm/dashboard',
  },
  {
    key: 'crm-customers',
    label: 'Accounts',
    isTitle: false,
    icon: 'mgc_building_2_line',
    url: '/crm/customers',
  },
  {
    key: 'crm-contacts',
    label: 'Contacts',
    isTitle: false,
    icon: 'mgc_contacts_line',
    url: '/crm/contacts',
  },
  {
    key: 'crm-leads',
    label: 'Leads',
    isTitle: false,
    icon: 'mgc_user_add_line',
    url: '/crm/leads',
  },
  {
    key: 'crm-opportunities',
    label: 'Opportunities',
    isTitle: false,
    icon: 'mgc_target_line',
    url: '/crm/opportunities',
  },
  {
    key: 'crm-quotes',
    label: 'Quotes',
    isTitle: false,
    icon: 'mgc_file_line',
    url: '/crm/quotes',
  },
  {
    key: 'crm-sales-orders',
    label: 'Orders',
    isTitle: false,
    icon: 'mgc_shopping_cart_1_line',
    url: '/crm/sales-orders',
  },
  {
    key: 'crm-activities',
    label: 'Activities',
    isTitle: false,
    icon: 'mgc_calendar_line',
    url: '/crm/activities',
  },
  {
    key: 'crm-campaigns',
    label: 'Campaigns',
    isTitle: false,
    icon: 'mgc_volume_line',
    url: '/crm/campaigns',
  },
  {
    key: 'crm-setup',
    label: 'Setup',
    isTitle: true,
  },
  {
    key: 'crm-customer-groups',
    label: 'Customer groups',
    isTitle: false,
    icon: 'mgc_group_line',
    url: '/crm/customer-groups',
  },
  {
    key: 'crm-price-lists',
    label: 'Price lists',
    isTitle: false,
    icon: 'mgc_currency_dollar_line',
    url: '/crm/price-lists',
  },
  {
    key: 'crm-territories',
    label: 'Territories',
    isTitle: false,
    icon: 'mgc_earth_line',
    url: '/crm/sales-territories',
  },
  {
    key: 'crm-returns',
    label: 'Return orders',
    isTitle: false,
    icon: 'mgc_refresh_1_line',
    url: '/crm/return-orders',
  },
];

/** @deprecated Use getMenuItemsForPath instead */
const MENU_ITEMS: MenuItemTypes[] = [...HOME_MENU, ...ERP_MENU, ...CRM_MENU];

function getMenuItemsForPath(pathname: string): MenuItemTypes[] {
  if (pathname.startsWith('/crm')) {
    return [MODULES_LINK, ...CRM_MENU];
  }

  if (pathname.startsWith('/erp') || pathname.startsWith('/core')) {
    return [MODULES_LINK, ...ERP_MENU];
  }

  return HOME_MENU;
}

export { MENU_ITEMS, HOME_MENU, ERP_MENU, CRM_MENU, getMenuItemsForPath };
