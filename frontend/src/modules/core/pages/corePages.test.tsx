import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";
import { beforeEach, describe, expect, it, vi } from "vitest";
import CoreDashboard from "./CoreDashboard";
import SettingsCategoryPage from "./SettingsCategoryPage";
import ModuleSelection from "../../../pages/modules/ModuleSelection";
import { MODULES } from "../../registry";
import { coreApi, type CoreDashboard as Dashboard, type SettingsCategory } from "../services/coreApi";

vi.mock("../services/coreApi", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../services/coreApi")>();
  return { ...actual, coreApi: { ...actual.coreApi, dashboard: vi.fn(), settings: vi.fn(), updateSettings: vi.fn(), context: vi.fn() } };
});

const renderAt = (element: React.ReactNode) =>
  render(
    <Provider store={configureStore({ reducer: { Auth: () => ({ user: { firstName: "Ayesha" } }) } })}>
      <RouterProvider router={createMemoryRouter([{ path: "*", element }])} />
    </Provider>
  );

beforeEach(() => {
  vi.mocked(coreApi.dashboard).mockReset();
  vi.mocked(coreApi.settings).mockReset();
  vi.mocked(coreApi.updateSettings).mockReset();
  vi.mocked(coreApi.context).mockResolvedValue({ companyName: "", currency: "USD", timeZone: "UTC", locale: "en-US", dateFormat: "dd MMM yyyy", firstDayOfWeek: 1 });
  sessionStorage.setItem("konrix_user", JSON.stringify({ token: "t", roleNames: ["Admin"] }));
});

describe("ModuleSelection", () => {
  it("renders every registry module as a keyboard-reachable link", () => {
    renderAt(<ModuleSelection />);
    for (const module of MODULES) {
      expect(screen.getByRole("link", { name: new RegExp(module.name.replace("/", "\\/")) })).toHaveAttribute("href", module.home);
    }
    expect(screen.getByText("Welcome back, Ayesha")).toBeInTheDocument();
  });
});

describe("CoreDashboard", () => {
  const dashboard: Dashboard = {
    users: { data: { totalUsers: 12, activeLast7Days: 9, activePrevious7Days: 6, lockedAccounts: 1 } },
    security: { error: "This figure couldn't be calculated right now." },
    errors: { data: { errorsLast24h: 0, errorsPrevious24h: 2, latest: [] } },
    configChanges: { data: [] },
    setup: { data: [{ key: "email", label: "Configure outgoing email", done: false, link: "/core/settings/email" }] },
    tenants: null,
  };

  it("shows real figures, and a failed widget only affects its own card", async () => {
    vi.mocked(coreApi.dashboard).mockResolvedValue(dashboard);
    renderAt(<CoreDashboard />);

    expect(await screen.findByText("9")).toBeInTheDocument();
    const failed = screen.getByText("Failed sign-ins (24h)").closest(".card") as HTMLElement;
    expect(within(failed).getByText("Not available yet")).toBeInTheDocument();
    expect(screen.getByText("No server errors")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Set up" })).toHaveAttribute("href", "/core/settings/email");
    expect(screen.queryByText("Tenants (platform)")).not.toBeInTheDocument();
  });

  it("offers retry when the whole summary fails", async () => {
    vi.mocked(coreApi.dashboard).mockRejectedValue(Object.assign(new Error("x"), { response: { status: 500, data: {} } }));
    renderAt(<CoreDashboard />);
    expect(await screen.findByRole("button", { name: /retry/i })).toBeInTheDocument();
  });
});

describe("SettingsCategoryPage", () => {
  const email: SettingsCategory = {
    category: "email",
    settings: [
      { key: "email.smtpHost", label: "SMTP server", type: "text", value: "", hasValue: false, isDefault: true, required: false, readOnly: false, max: 200 },
      { key: "email.smtpPassword", label: "SMTP password", type: "secret", value: null, hasValue: true, isDefault: false, required: false, readOnly: false },
    ],
  };

  it("never sends a stored secret it wasn't asked to change", async () => {
    vi.mocked(coreApi.settings).mockResolvedValue(email);
    vi.mocked(coreApi.updateSettings).mockResolvedValue(email);
    renderAt(<SettingsCategoryPage category="email" title="Email" description="Mail server" />);

    expect(await screen.findByText("Stored (hidden)")).toBeInTheDocument();
    await userEvent.type(screen.getByLabelText("SMTP server"), "smtp.x.test");
    await userEvent.click(screen.getByRole("button", { name: "Save settings" }));

    await waitFor(() => expect(coreApi.updateSettings).toHaveBeenCalledWith("email", { "email.smtpHost": "smtp.x.test" }));
  });

  it("clears a secret only when the user chooses Clear", async () => {
    vi.mocked(coreApi.settings).mockResolvedValue(email);
    vi.mocked(coreApi.updateSettings).mockResolvedValue(email);
    renderAt(<SettingsCategoryPage category="email" title="Email" description="Mail server" />);

    await userEvent.click(await screen.findByRole("button", { name: "Clear" }));
    expect(screen.getByText("Will be cleared when you save")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Save settings" }));

    await waitFor(() => expect(coreApi.updateSettings).toHaveBeenCalledWith("email", { "email.smtpHost": "", "email.smtpPassword": "" }));
  });

  it("puts catalog validation errors on the right field", async () => {
    vi.mocked(coreApi.settings).mockResolvedValue(email);
    vi.mocked(coreApi.updateSettings).mockRejectedValue(
      Object.assign(new Error("x"), { response: { status: 400, data: { message: "Some settings are not valid.", errors: { "": ["email.smtpHost: SMTP server cannot exceed 200 characters."] } } } })
    );
    renderAt(<SettingsCategoryPage category="email" title="Email" description="Mail server" />);

    await userEvent.click(await screen.findByRole("button", { name: "Save settings" }));

    expect(await screen.findByLabelText("SMTP server")).toHaveAccessibleDescription(/cannot exceed 200 characters/);
  });
});
