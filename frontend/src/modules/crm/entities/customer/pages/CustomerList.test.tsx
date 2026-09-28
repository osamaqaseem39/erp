import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import CustomerList from "./CustomerList";
import { customerService } from "../services";
import type { CustomerDto } from "../types";

vi.mock("../services", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../services")>();
  return {
    ...actual,
    customerService: { ...actual.customerService, list: vi.fn(), remove: vi.fn(), get: vi.fn(), update: vi.fn() },
  };
});

const customer = (n: number, overrides: Partial<CustomerDto> = {}): CustomerDto => ({
  id: `id-${n}`,
  companyId: "co",
  customerCode: `C-00${n}`,
  customerName: `Customer ${n}`,
  companyName: `Customer ${n}`,
  customerType: "Company",
  status: "Active",
  creditLimit: 1000 * n,
  outstandingBalance: 0,
  currencyCode: "USD",
  isActive: true,
  ...overrides,
});

const page = (items: CustomerDto[], totalCount = items.length) => ({
  items,
  totalCount,
  pageNumber: 1,
  pageSize: 25,
  totalPages: Math.ceil(totalCount / 25),
  hasPreviousPage: false,
  hasNextPage: totalCount > 25,
});

const list = vi.mocked(customerService.list);

function renderAt(url = "/crm/customers") {
  const router = createMemoryRouter(
    [
      { path: "/crm/customers", element: <CustomerList /> },
      { path: "/crm/customers/:id", element: <p>detail page</p> },
    ],
    { initialEntries: [url] }
  );
  render(<RouterProvider router={router} />);
  return router;
}

describe("CustomerList", () => {
  beforeEach(() => {
    list.mockReset();
    sessionStorage.setItem("konrix_user", JSON.stringify({ token: "t", roleNames: ["SuperAdmin"] }));
  });

  it("loads a server page and links each row to its detail page", async () => {
    list.mockResolvedValue(page([customer(1), customer(2)], 60));
    renderAt();

    expect(await screen.findByRole("link", { name: /Customer 1/ })).toHaveAttribute("href", "/crm/customers/id-1");
    expect(screen.getByText("Showing 1–25 of 60 customers")).toBeInTheDocument();
    expect(list).toHaveBeenCalledWith(expect.objectContaining({ pageNumber: 1, pageSize: 25, sortBy: "customerName", sortDirection: "asc" }), expect.any(AbortSignal));
  });

  it("sends the search to the server and keeps it in the URL", async () => {
    list.mockResolvedValue(page([customer(1)]));
    const router = renderAt();
    await screen.findByRole("link", { name: /Customer 1/ });

    await userEvent.type(screen.getByRole("searchbox", { name: "Search customers" }), "acme{Enter}");

    await waitFor(() => expect(list).toHaveBeenLastCalledWith(expect.objectContaining({ searchTerm: "acme", pageNumber: 1 }), expect.any(AbortSignal)));
    expect(router.state.location.search).toContain("q=acme");
  });

  it("restores filters and sort from the URL", async () => {
    list.mockResolvedValue(page([customer(1)]));
    renderAt("/crm/customers?status=Suspended&sort=creditLimit&dir=desc&page=2");

    await waitFor(() =>
      expect(list).toHaveBeenCalledWith(
        expect.objectContaining({ pageNumber: 2, sortBy: "creditLimit", sortDirection: "desc", filters: { status: "Suspended" } }),
        expect.any(AbortSignal)
      )
    );
    expect(await screen.findByRole("button", { name: "Remove filter Status: Suspended" })).toBeInTheDocument();
  });

  it("sorts on the server when a header is clicked", async () => {
    list.mockResolvedValue(page([customer(1)]));
    renderAt();
    await screen.findByRole("link", { name: /Customer 1/ });

    await userEvent.click(screen.getByRole("button", { name: /Credit limit/ }));

    await waitFor(() => expect(list).toHaveBeenLastCalledWith(expect.objectContaining({ sortBy: "creditLimit", sortDirection: "asc" }), expect.any(AbortSignal)));
  });

  it("shows why a customer can't be deleted", async () => {
    list.mockResolvedValue(page([customer(1)]));
    vi.mocked(customerService.remove).mockRejectedValue(
      Object.assign(new Error("x"), { response: { status: 409, data: { message: "It has 2 quotes. Mark it inactive instead to keep that history." } } })
    );
    renderAt();
    await screen.findByRole("link", { name: /Customer 1/ });

    await userEvent.click(screen.getByRole("button", { name: "Row actions" }));
    await userEvent.click(screen.getByRole("menuitem", { name: "Delete" }));
    const dialog = await screen.findByRole("dialog", { name: "Delete customer?" });
    expect(within(dialog).getByText("Customer 1 (C-001)")).toBeInTheDocument();
    await userEvent.click(within(dialog).getByRole("button", { name: "Delete customer" }));

    expect(await within(dialog).findByRole("alert")).toHaveTextContent("It has 2 quotes");
    expect(within(dialog).getByRole("button", { name: "Archive customer instead" })).toBeInTheDocument();
  });

  it("distinguishes an empty list from a search with no results", async () => {
    list.mockResolvedValue(page([]));
    const { unmount } = render(<RouterProvider router={createMemoryRouter([{ path: "*", element: <CustomerList /> }], { initialEntries: ["/crm/customers"] })} />);
    expect(await screen.findByText("No customers yet")).toBeInTheDocument();
    unmount();

    renderAt("/crm/customers?q=zzz");
    expect(await screen.findByText("No matching records")).toBeInTheDocument();
  });
});
