import { Menu, MenuButton, MenuItems } from "@headlessui/react";
import type { Column } from "./DataTable";

export function ColumnSelector<T>({
  columns,
  hidden,
  onToggle,
  onReset,
}: {
  columns: Column<T>[];
  hidden: ReadonlySet<string>;
  onToggle: (key: string) => void;
  onReset: () => void;
}) {
  const hideable = columns.filter((c) => c.hideable !== false);
  return (
    <Menu>
      <MenuButton className="btn border border-slate-300 dark:border-slate-600" aria-label="Choose columns">
        <i className="mgc_layout_grid_line" aria-hidden="true" />
        <span className="ms-1 hidden sm:inline">Columns</span>
      </MenuButton>
      <MenuItems
        anchor="bottom end"
        className="z-50 w-56 rounded-md border border-slate-200 bg-white p-2 shadow-lg focus:outline-none dark:border-slate-700 dark:bg-gray-800"
      >
        <p className="px-2 pb-1 text-xs font-semibold uppercase text-slate-500">Show columns</p>
        {hideable.map((column) => (
          <label key={column.key} className="flex cursor-pointer items-center gap-2 rounded px-2 py-1.5 text-sm hover:bg-slate-100 dark:hover:bg-slate-700">
            <input type="checkbox" className="form-checkbox rounded" checked={!hidden.has(column.key)} onChange={() => onToggle(column.key)} />
            {column.header}
          </label>
        ))}
        <button type="button" onClick={onReset} className="mt-1 w-full rounded px-2 py-1.5 text-left text-sm text-primary hover:bg-slate-100 dark:hover:bg-slate-700">
          Reset to default
        </button>
      </MenuItems>
    </Menu>
  );
}
