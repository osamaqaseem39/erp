import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { invoiceService, type InvoiceDto } from "../services";
import { customerService } from "../../../../crm/entities/customer/services";
import { INVOICE_STATUS_OPTIONS } from "../types";

const InvoiceList: React.FC = () => {
  const [invoices, setInvoices] = useState<InvoiceDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterCustomerId, setFilterCustomerId] = useState<string>("");
  const [filterStatus, setFilterStatus] = useState<string>("");
  const [customers, setCustomers] = useState<any[]>([]);

  useEffect(() => {
    loadCustomers();
  }, []);

  useEffect(() => {
    loadInvoices();
  }, [filterCustomerId, filterStatus]);

  const loadCustomers = async () => {
    try {
      const data = await customerService.getAll();
      setCustomers(data);
    } catch (err: any) {
      console.error("Failed to load customers", err);
    }
  };

  const loadInvoices = async () => {
    setLoading(true);
    setError(null);
    try {
      const query: any = {};
      if (filterCustomerId) query.customerId = filterCustomerId;
      if (filterStatus) query.status = filterStatus;
      
      const data = await invoiceService.getAll(query);
      setInvoices(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load invoices");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this invoice?")) return;

    try {
      await invoiceService.delete(id);
      loadInvoices();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete invoice");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = INVOICE_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Paid":
        return "bg-success";
      case "PartiallyPaid":
        return "bg-warning";
      case "Overdue":
        return "bg-danger";
      case "Draft":
        return "bg-secondary";
      case "Sent":
        return "bg-info";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("invoiceDate");
  const sortedInvoices = useMemo(
    () => sortRows(invoices),
    [invoices, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Invoices" subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Invoices</h4>
            <Link to="/finance/invoices/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Invoice
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2">
            <select
              className="form-select"
              value={filterCustomerId}
              onChange={(e) => setFilterCustomerId(e.target.value)}
            >
              <option value="">All Customers</option>
              {customers.map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.customerName}
                </option>
              ))}
            </select>

            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {INVOICE_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {loading ? (
            <div className="text-center py-8">Loading...</div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                <tr>
                <SortableTh columnKey="invoiceNumber" label="Invoice Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="invoiceDate" label="Invoice Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="dueDate" label="Due Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalAmount" label="Total Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="paidAmount" label="Paid Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="balanceAmount" label="Balance" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {invoices.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No invoices found
                      </td>
                    </tr>
                  ) : (
                    sortedInvoices.map((invoice) => (
                      <tr key={invoice.invoiceId || invoice.id}>
                        <td>{invoice.invoiceNumber}</td>
                        <td>{invoice.customerName || "-"}</td>
                        <td>{formatDate(invoice.invoiceDate)}</td>
                        <td>{formatDate(invoice.dueDate)}</td>
                        <td>{invoice.totalAmount.toFixed(2)}</td>
                        <td>{invoice.paidAmount.toFixed(2)}</td>
                        <td>{invoice.balanceAmount.toFixed(2)}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(invoice.status)}`}>
                            {getStatusLabel(invoice.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/finance/invoices/${invoice.invoiceId || invoice.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {invoice.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(invoice.invoiceId || invoice.id || "")}
                                className="btn btn-sm btn-soft-danger"
                              >
                                <i className="mgc_delete_line"></i>
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default InvoiceList;

