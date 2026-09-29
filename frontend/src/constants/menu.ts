import { APICore } from "../helpers/api/apiCore";
import { hasPermission, readPermissionContext } from "../lib/erp/permissions";
import { moduleForPath, type ErpModule } from "../modules/registry";

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
  key: "modules",
  label: "All modules",
  isTitle: false,
  icon: "mgc_apps_line",
  url: "/modules",
};

/** Shown on pages that don't belong to a module (e.g. the template demo pages in development). */
const HOME_MENU: MenuItemTypes[] = [{ key: "menu", label: "Menu", isTitle: true }, MODULES_LINK];

/**
 * Sidebar for a module, generated from modules/registry.ts. Entries the user may not open are left out
 * (permission codes are enforced from Checkpoint 2; SuperAdmin-only entries are hidden from others now).
 */
export function buildModuleMenu(module: ErpModule, session: Parameters<typeof readPermissionContext>[0]): MenuItemTypes[] {
  const permissions = readPermissionContext(session);
  const items: MenuItemTypes[] = [MODULES_LINK, { key: `${module.id}-title`, label: module.name, isTitle: true }];

  const homeIsListed = module.sections.some((s) => s.entries.some((e) => e.to === module.home));
  if (!homeIsListed) {
    items.push({ key: `${module.id}-home`, label: "Overview", icon: "mgc_home_3_line", url: module.home });
  }

  for (const section of module.sections) {
    const visible = section.entries.filter(
      (e) => (!e.superAdminOnly || permissions.isSuperAdmin) && (!e.permission || hasPermission(permissions, e.permission))
    );
    if (visible.length === 0) continue;
    items.push({ key: `${module.id}-${section.title}`, label: section.title, isTitle: true });
    items.push(...visible.map((e) => ({ key: `${module.id}-${e.key}`, label: e.label, icon: e.icon, url: e.to })));
  }
  return items;
}

export function getMenuItemsForPath(pathname: string): MenuItemTypes[] {
  const module = moduleForPath(pathname);
  return module ? buildModuleMenu(module, new APICore().getLoggedInUser()) : HOME_MENU;
}

export { HOME_MENU };
