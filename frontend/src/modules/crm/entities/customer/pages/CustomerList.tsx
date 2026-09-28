import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { EntityListLayout, LIST_TABLE_CLASS } from "../../../../../components";
import { customerService, type CustomerDto } from "../services";
import Swal from "sweetalert2";

const CustomerList: React.FC = () => {
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");

  useEffect(() => {
    loadCustomers();
  }, [statusFilter]);

  const loadCustomers = async () => {
    try {
      setLoading(true);
      let data: CustomerDto[];
      if (statusFilter) {
        data = await customerService.getByStatus(statusFilter);
      } else {
        data = await customerService.getAll();
      }
      setCustomers(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load customers");
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async () => {
    if (!searchTerm.trim()) {
      loadCustomers();
      return;
    }
    try {
      setLoading(true);
      const data = await customerService.search(searchTerm);
      setCustomers(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Search failed");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete customer ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await customerService.delete(id!);
        Swal.fire("Deleted!", "Customer has been deleted.", "success");
        loadCustomers();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete customer", "error");
      }
    }
  };

  const filteredCustomers = customers.filter((customer) => {
    if (searchTerm && !statusFilter) {
      const search = searchTerm.toLowerCase();
      return (
        customer.customerCode.toLowerCase().includes(search) ||
        customer.customerName.toLowerCase().includes(search) ||
        customer.companyName.toLowerCase().includes(search) ||
        customer.email?.toLowerCase().includes(search) ||
        customer.phone?.toLowerCase().includes(search)
      );
    }
    return true;
  });

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("customerName");
  const sortedCustomers = useMemo(
    () => sortRows(filteredCustomers),
    [filteredCustomers, sortRows]
  );

  return (
    <EntityListLayout
      title="Customers"
      moduleName="CRM"
      entityName="Customer"
      addUrl="/crm/customers/new"
      showBreadcrumb={false}
      error={error}
      toolbar={
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex-1 min-w-[200px]">
            <input
              type="text"
              className="form-input"
              placeholder="Search by code, name, company, email, or phone..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
            />
          </div>
          <select
            className="form-select w-auto"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">All Status</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="Suspended">Suspended</option>
          </select>
          <button type="button" onClick={handleSearch} className="btn btn-soft-primary">
            <i className="mgc_search_line me-2" aria-hidden />
            Search
          </button>
        </div>
      }
    >
      {loading ? (
        <div className="text-center py-8">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : (
        <div className="table-responsive">
          <table className={LIST_TABLE_CLASS}>
            <thead>
                <tr>
                <SortableTh columnKey="customerCode" label="Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="companyName" label="Company" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="email" label="Email" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="phone" label="Phone" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="creditLimit" label="Credit Limit" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-end" sortable={false} />
                </tr>
              </thead>
            <tbody>
              {filteredCustomers.length === 0 ? (
                <tr>
                  <td colSpan={9} className="text-center py-8 text-gray-500">
                    No customers found
                  </td>
                </tr>
              ) : (
                sortedCustomers.map((customer) => (
                  <tr key={customer.id}>
                    <td className="text-gray-500">{customer.customerCode}</td>
                    <td className="font-medium">{customer.customerName}</td>
                    <td>{customer.companyName}</td>
                    <td>{customer.customerType}</td>
                    <td>{customer.email || "—"}</td>
                    <td>{customer.phone || "—"}</td>
                    <td>
                      <span
                        className={`badge badge-pill ${
                          customer.status === "Active"
                            ? "badge-soft-success"
                            : customer.status === "Inactive"
                              ? "badge-soft-warning"
                              : "badge-soft-danger"
                        }`}
                      >
                        {customer.status}
                      </span>
                    </td>
                    <td>
                      {customer.currencyCode} {customer.creditLimit.toFixed(2)}
                    </td>
                    <td>
                      <div className="flex items-center justify-end gap-2">
                        <Link
                          to={`/crm/customers/${customer.id}/edit`}
                          className="btn btn-sm btn-soft-primary"
                          title="Edit"
                        >
                          <i className="mgc_edit_line" />
                        </Link>
                        <button
                          type="button"
                          onClick={() => handleDelete(customer.id!, customer.customerName)}
                          className="btn btn-sm btn-soft-danger"
                          title="Delete"
                        >
                          <i className="mgc_delete_line" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </EntityListLayout>
  );
};

export default CustomerList;
