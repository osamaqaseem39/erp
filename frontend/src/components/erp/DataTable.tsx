import { Menu, MenuButton, MenuItem, MenuItems } from "@headlessui/react";
import { useEffect, useRef, type ReactNode } from "react";
import { Link, useNavigate } from "react-router-dom";
import type { ApiErrorInfo } from "../../lib/erp/apiError";
import type { SortDirection } from "../../lib/erp/paging";
import { ErrorState } from "./ErrorState";
import { SkeletonTable } from "./Skeleton";

export interface Column<T> {
  /** Unique id; also the sort key sent to the API when `sortable`. */
  key: string;
  header: string;
  cell: (row: T) => ReactNode;
  sortable?: boolean;
  align?: "left" | "right" | "center";
  /** Can the user hide it from the column selector? Default true. */
  hideable?: boolean;
  /** Hidden until the user turns it on. */
  defaultHidden?: boolean;
  className?: string;
}

export interface RowAction<T> {
  label: string;
  icon?: string;
  onSelect: (row: T) => void;
  tone?: "default" | "danger";
  /** Omit the action for this row (e.g. no Delete on posted documents). */
  hidden?: (row: T) => boolean;
}

export interface SortState {
  key: string;
  direction: SortDirection;
}

export interface DataTableProps<T> {
  /** Screen-reader caption, e.g. "Customers". */
  caption: string;
  columns: Column<T>[];
  rows: T[] | undefined;
  getRowId: (row: T) => string;
  /** Keys of columns currently hidden (see useColumnVisibility). */
  hiddenColumns?: ReadonlySet<string>;
  isLoading?: boolean;
  isRefreshing?: boolean;
  error?: ApiErrorInfo | null;
  onRetry?: () => void;
  sort?: SortState;
  onSortChange?: (sort: SortState) => void;
  /** Detail page URL; makes the first column a link and the row clickable. */
  rowHref?: (row: T) => string;
  rowActions?: RowAction<T>[];
  selectedIds?: ReadonlySet<string>;
  onSelectionChange?: (ids: Set<string>) => void;
  /** Shown when there are no rows at all. */
  emptyState: ReactNode;
  /** Shown instead when filters/search are active and nothing matches. */
  noResultsState?: ReactNode;
  isFiltered?: boolean;
}

const alignClass = { left: "text-left", right: "text-right", center: "text-center" } as const;

export function DataTable<T>({
  caption,
  columns,
  rows,
  getRowId,
  hiddenColumns,
  isLoading,
  isRefreshing,
  error,
  onRetry,
  sort,
  onSortChange,
  rowHref,
  rowActions,
  selectedIds,
  onSelectionChange,
  emptyState,
  noResultsState,
  isFiltered,
}: DataTableProps<T>) {
  const navigate = useNavigate();
  const visible = columns.filter((c) => !hiddenColumns?.has(c.key));
  const selectable = !!onSelectionChange;
  const hasActions = !!rowActions?.length;
  const columnCount = visible.length + (selectable ? 1 : 0) + (hasActions ? 1 : 0);

  if (isLoading) return <SkeletonTable columns={Math.min(visible.length, 6)} />;
  if (error && !rows) return <ErrorState error={error} onRetry={onRetry} />;
  if (rows && rows.length === 0) return <>{isFiltered && noResultsState ? noResultsState : emptyState}</>;

  const data = rows ?? [];
  const pageIds = data.map(getRowId);
  const selectedOnPage = pageIds.filter((id) => selectedIds?.has(id)).length;

  const toggleAll = () => {
    const next = new Set(selectedIds);
    if (selectedOnPage === pageIds.length) pageIds.forEach((id) => next.delete(id));
    else pageIds.forEach((id) => next.add(id));
    onSelectionChange?.(next);
  };
  const toggleOne = (id: string) => {
    const next = new Set(selectedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onSelectionChange?.(next);
  };

  const onHeaderSort = (column: Column<T>) => {
    if (!column.sortable || !onSortChange) return;
    const direction: SortDirection = sort?.key === column.key && sort.direction === "asc" ? "desc" : "asc";
    onSortChange({ key: column.key, direction });
  };

  return (
    <div className="relative">
      {error && (
        <div role="alert" className="border-b border-danger/20 bg-danger/5 px-4 py-2 text-sm text-danger">
          {error.message}{" "}
          {onRetry && (
            <button type="button" className="font-medium underline" onClick={onRetry}>
              Retry
            </button>
          )}
        </div>
      )}
      <div className={`overflow-x-auto ${isRefreshing ? "opacity-60 transition-opacity" : ""}`} aria-busy={isRefreshing}>
        <table className="min-w-full text-sm">
          <caption className="sr-only">{caption}</caption>
          <thead className="sticky top-0 z-10 bg-slate-50 dark:bg-slate-800">
            <tr className="border-b border-slate-200 dark:border-slate-700">
              {selectable && (
                <th scope="col" className="w-10 px-3 py-2.5">
                  <IndeterminateCheckbox
                    checked={pageIds.length > 0 && selectedOnPage === pageIds.length}
                    indeterminate={selectedOnPage > 0 && selectedOnPage < pageIds.length}
                    onChange={toggleAll}
                    label="Select all rows on this page"
                  />
                </th>
              )}
              {visible.map((column) => {
                const active = sort?.key === column.key;
                const ariaSort = active ? (sort.direction === "asc" ? "ascending" : "descending") : undefined;
                return (
                  <th
                    key={column.key}
                    scope="col"
                    aria-sort={column.sortable ? ariaSort ?? "none" : undefined}
                    className={`whitespace-nowrap px-3 py-2.5 normal-case tracking-normal font-semibold text-slate-600 dark:text-slate-300 ${alignClass[column.align ?? "left"]} ${column.className ?? ""}`}
                  >
                    {column.sortable && onSortChange ? (
                      <button
                        type="button"
                        onClick={() => onHeaderSort(column)}
                        className="inline-flex items-center gap-1 hover:text-primary focus-visible:text-primary"
                      >
                        {column.header}
                        <i
                          className={
                            active ? (sort.direction === "asc" ? "mgc_arrow_up_line" : "mgc_arrow_down_line") : "mgc_sort_ascending_line opacity-30"
                          }
                          aria-hidden="true"
                        />
                      </button>
                    ) : (
                      column.header
                    )}
                  </th>
                );
              })}
              {hasActions && (
                <th scope="col" className="w-12 px-3 py-2.5">
                  <span className="sr-only">Actions</span>
                </th>
              )}
            </tr>
          </thead>
          <tbody>
            {data.map((row) => {
              const id = getRowId(row);
              const href = rowHref?.(row);
              const isSelected = selectedIds?.has(id) ?? false;
              return (
                <tr
                  key={id}
                  className={`border-b border-slate-100 dark:border-slate-700/60 ${href ? "cursor-pointer" : ""} ${
                    isSelected ? "bg-primary/5" : "hover:bg-slate-50 dark:hover:bg-slate-800/60"
                  }`}
                  onClick={(e) => {
                    // Mouse convenience only; keyboard users use the link in the first column.
                    if (!href || (e.target as HTMLElement).closest("a,button,input,[role=menu]")) return;
                    navigate(href);
                  }}
                >
                  {selectable && (
                    <td className="px-3 py-2.5">
                      <input
                        type="checkbox"
                        className="form-checkbox rounded"
                        checked={isSelected}
                        onChange={() => toggleOne(id)}
                        aria-label="Select row"
                      />
                    </td>
                  )}
                  {visible.map((column, index) => (
                    <td
                      key={column.key}
                      className={`px-3 py-2.5 text-slate-700 dark:text-slate-200 ${alignClass[column.align ?? "left"]} ${column.className ?? ""}`}
                    >
                      {index === 0 && href ? (
                        <Link to={href} className="font-medium text-slate-900 hover:text-primary dark:text-slate-100">
                          {column.cell(row)}
                        </Link>
                      ) : (
                        column.cell(row)
                      )}
                    </td>
                  ))}
                  {hasActions && (
                    <td className="px-3 py-1.5 text-right">
                      <RowActionsMenu row={row} actions={rowActions!} />
                    </td>
                  )}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
      {columnCount === 0 && <p className="p-4 text-sm text-slate-500">All columns are hidden.</p>}
    </div>
  );
}

function RowActionsMenu<T>({ row, actions }: { row: T; actions: RowAction<T>[] }) {
  const available = actions.filter((a) => !a.hidden?.(row));
  if (available.length === 0) return null;
  return (
    <Menu>
      <MenuButton
        className="inline-flex h-8 w-8 items-center justify-center rounded hover:bg-slate-100 focus-visible:ring-2 focus-visible:ring-primary dark:hover:bg-slate-700"
        aria-label="Row actions"
      >
        <i className="mgc_more_2_fill text-lg" aria-hidden="true" />
      </MenuButton>
      <MenuItems
        anchor="bottom end"
        className="z-50 min-w-40 rounded-md border border-slate-200 bg-white py-1 shadow-lg focus:outline-none dark:border-slate-700 dark:bg-gray-800"
      >
        {available.map((action) => (
          <MenuItem key={action.label}>
            <button
              type="button"
              onClick={() => action.onSelect(row)}
              className={`flex w-full items-center gap-2 px-3 py-1.5 text-left text-sm data-[focus]:bg-slate-100 dark:data-[focus]:bg-slate-700 ${
                action.tone === "danger" ? "text-danger" : "text-slate-700 dark:text-slate-200"
              }`}
            >
              {action.icon && <i className={action.icon} aria-hidden="true" />}
              {action.label}
            </button>
          </MenuItem>
        ))}
      </MenuItems>
    </Menu>
  );
}

function IndeterminateCheckbox({
  checked,
  indeterminate,
  onChange,
  label,
}: {
  checked: boolean;
  indeterminate: boolean;
  onChange: () => void;
  label: string;
}) {
  const ref = useRef<HTMLInputElement>(null);
  useEffect(() => {
    if (ref.current) ref.current.indeterminate = indeterminate;
  }, [indeterminate]);
  return <input ref={ref} type="checkbox" className="form-checkbox rounded" checked={checked} onChange={onChange} aria-label={label} />;
}
