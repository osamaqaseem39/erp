import { useId } from "react";
import { PAGE_SIZE_OPTIONS, pageRange, visiblePages } from "../../lib/erp/paging";

export interface PaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
  /** Singular/plural noun for the summary, e.g. ["customer", "customers"]. */
  noun?: [string, string];
}

export function Pagination({ pageNumber, pageSize, totalCount, onPageChange, onPageSizeChange, noun = ["record", "records"] }: PaginationProps) {
  const sizeId = useId();
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const { from, to } = pageRange({ pageNumber, pageSize, totalCount });
  const pages = visiblePages(pageNumber, totalPages);

  const button = "inline-flex h-8 min-w-8 items-center justify-center rounded px-2 text-sm disabled:opacity-40";

  return (
    <nav aria-label="Pagination" className="flex flex-wrap items-center justify-between gap-3 px-4 py-3 text-sm">
      <p className="text-slate-600 dark:text-slate-400" aria-live="polite">
        {totalCount === 0 ? `No ${noun[1]}` : `Showing ${from}–${to} of ${totalCount.toLocaleString()} ${totalCount === 1 ? noun[0] : noun[1]}`}
      </p>
      <div className="flex items-center gap-3">
        {onPageSizeChange && (
          <div className="flex items-center gap-2">
            <label htmlFor={sizeId} className="text-slate-600 dark:text-slate-400">
              Rows
            </label>
            <select
              id={sizeId}
              className="form-select py-1 pe-8 text-sm"
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
            >
              {PAGE_SIZE_OPTIONS.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </div>
        )}
        <div className="flex items-center gap-1">
          <button
            type="button"
            className={`${button} hover:bg-slate-100 dark:hover:bg-slate-700`}
            onClick={() => onPageChange(pageNumber - 1)}
            disabled={pageNumber <= 1}
            aria-label="Previous page"
          >
            <i className="mgc_left_line" aria-hidden="true" />
          </button>
          {pages.map((page, index) =>
            page === null ? (
              <span key={`gap-${index}`} className="px-1 text-slate-400" aria-hidden="true">
                …
              </span>
            ) : (
              <button
                key={page}
                type="button"
                className={`${button} ${page === pageNumber ? "bg-primary text-white" : "hover:bg-slate-100 dark:hover:bg-slate-700"}`}
                onClick={() => onPageChange(page)}
                aria-current={page === pageNumber ? "page" : undefined}
                aria-label={`Page ${page}`}
              >
                {page}
              </button>
            )
          )}
          <button
            type="button"
            className={`${button} hover:bg-slate-100 dark:hover:bg-slate-700`}
            onClick={() => onPageChange(pageNumber + 1)}
            disabled={pageNumber >= totalPages}
            aria-label="Next page"
          >
            <i className="mgc_right_line" aria-hidden="true" />
          </button>
        </div>
      </div>
    </nav>
  );
}

