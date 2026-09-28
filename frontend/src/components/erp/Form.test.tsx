import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { FormProvider, useForm } from "react-hook-form";
import { describe, expect, it } from "vitest";
import { applyServerErrors } from "../../lib/erp/formErrors";
import { parseApiError } from "../../lib/erp/apiError";
import { FormErrorSummary, NumberField, TextField } from "./Form";
import { useState } from "react";

function Harness({ serverError }: { serverError?: unknown }) {
  const methods = useForm<{ email: string; creditLimit?: number }>({ defaultValues: { email: "" } });
  const [summary, setSummary] = useState<string[]>([]);
  const [submitted, setSubmitted] = useState<unknown>(null);
  return (
    <FormProvider {...methods}>
      <form
        onSubmit={methods.handleSubmit((values) => {
          setSubmitted(values);
          if (serverError) setSummary(applyServerErrors(parseApiError(serverError), methods.setError, ["email", "creditLimit"]));
        })}
      >
        <FormErrorSummary messages={summary} />
        <TextField name="email" label="Email" required help="We send invoices here." />
        <NumberField name="creditLimit" label="Credit limit" />
        <button type="submit">Save</button>
        <output data-testid="submitted">{JSON.stringify(submitted)}</output>
      </form>
    </FormProvider>
  );
}

describe("form fields", () => {
  it("connects label, required marker and help text to the input", () => {
    render(<Harness />);
    const input = screen.getByRole("textbox", { name: /email/i });
    expect(input).toHaveAttribute("aria-required", "true");
    expect(input).toHaveAccessibleDescription("We send invoices here.");
  });

  it("keeps numbers as numbers and empty as undefined", async () => {
    render(<Harness />);
    await userEvent.type(screen.getByRole("spinbutton", { name: "Credit limit" }), "1500");
    await userEvent.click(screen.getByRole("button", { name: "Save" }));
    expect(screen.getByTestId("submitted").textContent).toBe('{"email":"","creditLimit":1500}');
  });

  it("puts server field errors on the field and the rest in the summary", async () => {
    const serverError = Object.assign(new Error("x"), {
      response: { status: 400, data: { message: "Validation failed", errors: { email: ["Email is already registered"], "": ["Credit check failed"] } } },
    });
    render(<Harness serverError={serverError} />);
    await userEvent.click(screen.getByRole("button", { name: "Save" }));

    const input = await screen.findByRole("textbox", { name: /email/i });
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(input).toHaveAccessibleDescription(/Email is already registered/);
    expect(screen.getByRole("alert").textContent).toContain("Credit check failed");
  });

  it("falls back to the general message when no field matches", async () => {
    const serverError = Object.assign(new Error("x"), { response: { status: 409, data: { message: "Customer code already exists." } } });
    render(<Harness serverError={serverError} />);
    await userEvent.click(screen.getByRole("button", { name: "Save" }));
    expect((await screen.findByRole("alert")).textContent).toContain("Customer code already exists.");
  });
});
