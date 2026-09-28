import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { billService, type BillDto } from "../services";
import { BILL_STATUS_OPTIONS } from "../types";

const BillList: React.FC = () => {
  const [bills, setBills] = useState<BillDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadBills();
  }, [filterStatus]);

  const loadBills = async () => {
    setLoading(true);
    setError(null);
    try {
      const query: any = {};
      if (filterStatus) query.status = filterStatus;
      
      const data = await billService.getAll(query);
      setBills(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load bills");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this bill?")) return;

    try {
      await billService.delete(id);
      loadBills();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete bill");
    }
  };

  const handleApprove = async (id: string) => {
    if (!window.confirm("Are you sure you want to approve this bill?")) return;

    try {
      await billService.approve(id);
      loadBills();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to approve bill");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = BILL_STATUS_OPTIONS.find((opt) => opt.value === status);
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
      case "Approved":
        return "bg-info";
      case "Issued":
        return "bg-primary";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("billDate");
  const sortedBills = useMemo(
    () => sortRows(bills),
    [bills, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Bills" subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Bills</h4>
            <Link to="/finance/bills/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Bill
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4">
            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {BILL_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="billNumber" label="Bill Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="vendorName" label="Vendor" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="billDate" label="Bill Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="dueDate" label="Due Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalAmount" label="Total Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {bills.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center py-8">
                        No bills found
                      </td>
                    </tr>
                  ) : (
                    sortedBills.map((bill) => (
                      <tr key={bill.billId || bill.id}>
                        <td>{bill.billNumber}</td>
                        <td>{bill.vendorName || bill.vendorId}</td>
                        <td>{formatDate(bill.billDate)}</td>
                        <td>{formatDate(bill.dueDate)}</td>
                        <td>{bill.totalAmount.toFixed(2)}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(bill.status)}`}>
                            {getStatusLabel(bill.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/finance/bills/${bill.billId || bill.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {bill.status === "Draft" && (
                              <button
                                onClick={() => handleApprove(bill.billId || bill.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Approve Bill"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {bill.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(bill.billId || bill.id || "")}
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

export default BillList;

