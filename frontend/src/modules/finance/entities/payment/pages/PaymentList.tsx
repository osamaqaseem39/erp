import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { paymentService, type PaymentDto } from "../services";
import { PAYMENT_METHOD_OPTIONS, PAYMENT_STATUS_OPTIONS } from "../types";

const PaymentList: React.FC = () => {
  const [payments, setPayments] = useState<PaymentDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");
  const [filterMethod, setFilterMethod] = useState<string>("");

  useEffect(() => {
    loadPayments();
  }, [filterStatus, filterMethod]);

  const loadPayments = async () => {
    setLoading(true);
    setError(null);
    try {
      const query: any = {};
      if (filterStatus) query.status = filterStatus;
      if (filterMethod) query.paymentMethod = filterMethod;
      
      const data = await paymentService.getAll(query);
      setPayments(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load payments");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this payment?")) return;

    try {
      await paymentService.delete(id);
      loadPayments();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete payment");
    }
  };

  const getMethodLabel = (method?: string) => {
    const option = PAYMENT_METHOD_OPTIONS.find((opt) => opt.value === method);
    return option?.label || method || "N/A";
  };

  const getStatusLabel = (status?: string) => {
    const option = PAYMENT_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "Pending":
        return "bg-warning";
      case "Failed":
        return "bg-danger";
      case "Reconciled":
        return "bg-info";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("paymentDate");
  const sortedPayments = useMemo(
    () => sortRows(payments),
    [payments, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Payments" subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Payments</h4>
            <Link to="/finance/payments/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Payment
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2">
            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {PAYMENT_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              className="form-select"
              value={filterMethod}
              onChange={(e) => setFilterMethod(e.target.value)}
            >
              <option value="">All Methods</option>
              {PAYMENT_METHOD_OPTIONS.map((option) => (
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
                <SortableTh columnKey="paymentNumber" label="Payment Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="paymentDate" label="Payment Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="paymentMethod" label="Payment Method" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="amount" label="Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="invoiceNumber" label="Related Invoice" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="billNumber" label="Related Bill" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {payments.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-8">
                        No payments found
                      </td>
                    </tr>
                  ) : (
                    sortedPayments.map((payment) => (
                      <tr key={payment.paymentId || payment.id}>
                        <td>{payment.paymentNumber || "-"}</td>
                        <td>{formatDate(payment.paymentDate)}</td>
                        <td>{getMethodLabel(payment.paymentMethod)}</td>
                        <td>{payment.amount?.toFixed(2) || "0.00"}</td>
                        <td>{payment.relatedInvoiceNumber || "-"}</td>
                        <td>{payment.relatedBillNumber || "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(payment.status)}`}>
                            {getStatusLabel(payment.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/finance/payments/${payment.paymentId || payment.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {payment.status === "Pending" && (
                              <button
                                onClick={() => handleDelete(payment.paymentId || payment.id || "")}
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

export default PaymentList;

