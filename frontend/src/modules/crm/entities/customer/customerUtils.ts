import { customerService } from "./services";
import type { CustomerDto, UpdateCustomerDto } from "./types";

/** "Acme Ltd (C-001)": how a customer is named in confirmations and messages. */
export const customerDisplayName = (c: Pick<CustomerDto, "customerName" | "customerCode">) =>
  c.customerCode ? `${c.customerName} (${c.customerCode})` : c.customerName;

/** Full update DTO from a loaded customer, so partial changes never blank other fields. */
export function toUpdateDto(c: CustomerDto): UpdateCustomerDto {
  return {
    id: c.id!,
    customerCode: c.customerCode,
    customerName: c.customerName,
    companyName: c.companyName,
    contactPerson: c.contactPerson,
    email: c.email,
    phone: c.phone,
    address: c.address,
    city: c.city,
    state: c.state,
    country: c.country,
    postalCode: c.postalCode,
    customerType: c.customerType,
    status: c.status,
    industry: c.industry,
    creditLimit: c.creditLimit,
    outstandingBalance: c.outstandingBalance,
    paymentTerms: c.paymentTerms,
    taxId: c.taxId,
    website: c.website,
    notes: c.notes,
    currencyCode: c.currencyCode,
  };
}

/** The non-destructive alternative to deleting a customer with history. */
export async function markCustomerInactive(customer: CustomerDto): Promise<void> {
  // Re-read so the update carries the latest values, not a possibly stale list row.
  const latest = await customerService.get(customer.id!);
  await customerService.update(latest.id!, { ...toUpdateDto(latest), status: "Inactive" });
}
