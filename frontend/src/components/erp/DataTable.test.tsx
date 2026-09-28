import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { DataTable, type Column } from "./DataTable";

interface Row {
  id: string;
  name: string;
  status: string;
}

const rows: Row[] = [
  { id: "1", name: "Acme", status: "Active" },
  { id: "2", name: "Globex", status: "Inactive" },
];

const columns: Column<Row>[] = [
  { key: "name", header: "Name", cell: (r) => r.name, sortable: true },
  { key: "status", header: "Status", cell: (r) => r.status },
];

const renderTable = (props: Partial<Parameters<typeof DataTable<Row>>[0]> = {}) =>
  render(
    <MemoryRouter>
      <DataTable<Row>
        caption="Customers"
        columns={columns}
        rows={rows}
        getRowId={(r) => r.id}
        emptyState={<p>No customers yet</p>}
        {...props}
      />
    </MemoryRouter>
  );

describe("DataTable", () => {
  it("renders a captioned table with rows", () => {
    renderTable();
    const table = screen.getByRole("table", { name: "Customers" });
    expect(within(table).getAllByRole("row")).toHaveLength(3);
  });

  it("requests the next sort direction and exposes aria-sort", async () => {
    const onSortChange = vi.fn();
    renderTable({ sort: { key: "name", direction: "asc" }, onSortChange });
    expect(screen.getByRole("columnheader", { name: /name/i })).toHaveAttribute("aria-sort", "ascending");
    await userEvent.click(screen.getByRole("button", { name: /name/i }));
    expect(onSortChange).toHaveBeenCalledWith({ key: "name", direction: "desc" });
  });

  it("links the first column to the detail page", () => {
    renderTable({ rowHref: (r) => `/crm/customers/${r.id}` });
    expect(screen.getByRole("link", { name: "Acme" })).toHaveAttribute("href", "/crm/customers/1");
  });

  it("selects all rows on the page and reports partial selection", async () => {
    const onSelectionChange = vi.fn();
    const { rerender } = renderTable({ selectedIds: new Set(), onSelectionChange });
    await userEvent.click(screen.getByRole("checkbox", { name: /select all/i }));
    expect(onSelectionChange).toHaveBeenCalledWith(new Set(["1", "2"]));

    rerender(
      <MemoryRouter>
        <DataTable<Row>
          caption="Customers"
          columns={columns}
          rows={rows}
          getRowId={(r) => r.id}
          emptyState={null}
          selectedIds={new Set(["1"])}
          onSelectionChange={onSelectionChange}
        />
      </MemoryRouter>
    );
    expect((screen.getByRole("checkbox", { name: /select all/i }) as HTMLInputElement).indeterminate).toBe(true);
  });

  it("offers row actions in a keyboard menu and hides unavailable ones", async () => {
    const onDelete = vi.fn();
    renderTable({
      rowActions: [
        { label: "Edit", onSelect: vi.fn() },
        { label: "Delete", tone: "danger", onSelect: onDelete, hidden: (r) => r.status === "Inactive" },
      ],
    });
    const [firstMenu, secondMenu] = screen.getAllByRole("button", { name: "Row actions" });
    await userEvent.click(secondMenu);
    expect(screen.queryByRole("menuitem", { name: "Delete" })).toBeNull();
    await userEvent.keyboard("{Escape}");

    await userEvent.click(firstMenu);
    await userEvent.click(screen.getByRole("menuitem", { name: "Delete" }));
    expect(onDelete).toHaveBeenCalledWith(rows[0]);
  });

  it("shows the empty state, or the no-results state when filtered", () => {
    const { unmount } = renderTable({ rows: [] });
    expect(screen.getByText("No customers yet")).toBeTruthy();
    unmount();
    renderTable({ rows: [], isFiltered: true, noResultsState: <p>Nothing matches</p> });
    expect(screen.getByText("Nothing matches")).toBeTruthy();
  });

  it("shows a skeleton while loading and an error with retry when the first load fails", async () => {
    const { unmount } = renderTable({ rows: undefined, isLoading: true });
    expect(screen.getByRole("status", { name: "Loading" })).toBeTruthy();
    unmount();

    const onRetry = vi.fn();
    renderTable({ rows: undefined, error: { status: 500, message: "Boom", code: null, fieldErrors: {}, traceId: "t1", isNetworkError: false, isCanceled: false }, onRetry });
    expect(screen.getByRole("alert").textContent).toContain("Boom");
    await userEvent.click(screen.getByRole("button", { name: /retry/i }));
    expect(onRetry).toHaveBeenCalled();
  });

  it("hides columns the user turned off", () => {
    renderTable({ hiddenColumns: new Set(["status"]) });
    expect(screen.queryByRole("columnheader", { name: "Status" })).toBeNull();
  });
});
