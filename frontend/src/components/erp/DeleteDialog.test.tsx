import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { DeleteDialog } from "./DeleteDialog";

const setup = (overrides: Partial<Parameters<typeof DeleteDialog>[0]> = {}) => {
  const props = {
    open: true,
    entityName: "customer",
    recordLabel: "Acme Ltd (C-001)",
    onDelete: vi.fn(() => Promise.resolve()),
    onDone: vi.fn(),
    onCancel: vi.fn(),
    ...overrides,
  };
  render(<DeleteDialog {...props} />);
  return props;
};

describe("DeleteDialog", () => {
  it("names the record and focuses Cancel so Enter never deletes by accident", async () => {
    setup();
    expect(screen.getByRole("dialog", { name: "Delete customer?" })).toBeTruthy();
    expect(screen.getByText("Acme Ltd (C-001)")).toBeTruthy();
    await waitFor(() => expect(screen.getByRole("button", { name: "Cancel" })).toHaveFocus());
  });

  it("deletes and reports completion", async () => {
    const props = setup();
    await userEvent.click(screen.getByRole("button", { name: "Delete customer" }));
    await waitFor(() => expect(props.onDone).toHaveBeenCalledWith("deleted"));
  });

  it("shows a backend refusal inside the dialog instead of closing", async () => {
    const conflict = Object.assign(new Error("x"), {
      response: { status: 409, data: { message: "It has 3 open sales orders." } },
    });
    const props = setup({ onDelete: vi.fn(() => Promise.reject(conflict)) });
    await userEvent.click(screen.getByRole("button", { name: "Delete customer" }));
    expect((await screen.findByRole("alert")).textContent).toContain("It has 3 open sales orders.");
    expect(props.onDone).not.toHaveBeenCalled();
  });

  it("requires typing the record label when configured", async () => {
    setup({ requireTypedConfirmation: true });
    const confirm = screen.getByRole("button", { name: "Delete customer" });
    expect(confirm).toBeDisabled();
    await userEvent.type(screen.getByLabelText(/type/i), "Acme Ltd (C-001)");
    expect(confirm).toBeEnabled();
  });

  it("offers archive as the safer alternative", async () => {
    const onArchive = vi.fn(() => Promise.resolve());
    const props = setup({ onArchive });
    await userEvent.click(screen.getByRole("button", { name: "Archive customer instead" }));
    await waitFor(() => expect(props.onDone).toHaveBeenCalledWith("archived"));
  });

  it("closes on Escape", async () => {
    const props = setup();
    await userEvent.keyboard("{Escape}");
    expect(props.onCancel).toHaveBeenCalled();
  });
});
