import { Link } from "react-router-dom";
import { PageHeader, StatusBadge } from "../../../components/erp";

interface ReferenceList {
  name: string;
  description: string;
  /** Existing page that manages the list, if there is one. */
  to?: string;
  owner: string;
  /** When there is no page yet: the checkpoint that adds it. */
  arrives?: string;
}

/**
 * Shared lists other modules pick from. Each list is managed by the module that owns it; this page
 * links to those screens instead of duplicating them, and says plainly which ones don't exist yet.
 */
const LISTS: ReferenceList[] = [
  { name: "Currencies", description: "Codes, symbols and which are active.", to: "/finance/currencies", owner: "Finance" },
  { name: "Units of measure", description: "Pieces, kilograms, hours and conversions.", to: "/inventory/units-of-measure", owner: "Inventory" },
  { name: "Item categories", description: "Grouping for items and reports.", to: "/inventory/item-categories", owner: "Inventory" },
  { name: "Customer groups", description: "Segments with default discounts.", to: "/crm/customer-groups", owner: "CRM" },
  { name: "Price lists", description: "Named price sets for customers.", to: "/crm/price-lists", owner: "CRM" },
  { name: "Tax rates", description: "Sales and purchase tax rates.", owner: "Finance", arrives: "Checkpoint 6" },
  { name: "Payment methods", description: "Cash, card, bank transfer and others.", owner: "Finance", arrives: "Checkpoint 6" },
  { name: "Cost centers", description: "For allocating income and expenses.", owner: "Finance", arrives: "Checkpoint 6" },
  { name: "Departments", description: "Organization structure for people and cost.", to: "/hrm/departments", owner: "HRM" },
];

export default function ReferenceDataPage() {
  return (
    <>
      <PageHeader
        title="Reference data"
        description="Shared lists used across modules. Each is managed by the module that owns it."
        breadcrumbs={[{ label: "Core / Settings", to: "/core" }, { label: "Settings", to: "/core/settings" }, { label: "Reference data" }]}
      />
      <div className="card overflow-x-auto">
        <table className="min-w-full text-sm">
          <caption className="sr-only">Reference lists</caption>
          <thead className="bg-slate-50 dark:bg-slate-800">
            <tr>
              <th scope="col" className="px-4 py-2.5 text-left font-semibold normal-case tracking-normal">List</th>
              <th scope="col" className="px-4 py-2.5 text-left font-semibold normal-case tracking-normal">Managed in</th>
              <th scope="col" className="px-4 py-2.5 text-left font-semibold normal-case tracking-normal">Status</th>
            </tr>
          </thead>
          <tbody>
            {LISTS.map((list) => (
              <tr key={list.name} className="border-t border-slate-100 dark:border-slate-700">
                <td className="px-4 py-3">
                  {list.to ? (
                    <Link to={list.to} className="font-medium text-primary hover:underline">
                      {list.name}
                    </Link>
                  ) : (
                    <span className="font-medium">{list.name}</span>
                  )}
                  <span className="block text-slate-500">{list.description}</span>
                </td>
                <td className="px-4 py-3">{list.owner}</td>
                <td className="px-4 py-3">
                  {list.to ? <StatusBadge status="Active" labels={{ active: "Available" }} /> : <span className="text-slate-500">Arrives in {list.arrives}</span>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
