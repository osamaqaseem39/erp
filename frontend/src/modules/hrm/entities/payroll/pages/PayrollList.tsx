import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { payrollService, type PayrollDto } from "../services";
import { PAYROLL_STATUS_OPTIONS } from "../types";

const PayrollList: React.FC = () => {
  const [payrolls, setPayrolls] = useState<PayrollDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadPayrolls();
  }, [filterStatus]);

  const loadPayrolls = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await payrollService.getAll();
      let filtered = data;
      if (filterStatus) {
        filtered = data.filter((payroll) => payroll.status === filterStatus);
      }
      setPayrolls(filtered);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load payrolls");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this payroll?")) return;

    try {
      await payrollService.delete(id);
      loadPayrolls();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete payroll");
    }
  };

  const handleProcess = async (id: string) => {
    if (!window.confirm("Are you sure you want to process this payroll?")) return;

    try {
      await payrollService.process(id);
      loadPayrolls();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to process payroll");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = PAYROLL_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Paid":
        return "bg-success";
      case "Approved":
        return "bg-info";
      case "Processed":
        return "bg-primary";
      case "Processing":
        return "bg-warning";
      case "Draft":
        return "bg-secondary";
      case "Cancelled":
        return "bg-danger";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("payDate");
  const sortedPayrolls = useMemo(
    () => sortRows(payrolls),
    [payrolls, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Payrolls" subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Payrolls</h4>
            <Link to="/hrm/payrolls/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Payroll
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
              {PAYROLL_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="payrollNumber" label="Payroll Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="payPeriod" label="Pay Period" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="payDate" label="Pay Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="employeeCount" label="Employee Count" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalGross" label="Total Gross" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalDeductions" label="Total Deductions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalNet" label="Total Net" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {payrolls.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No payrolls found
                      </td>
                    </tr>
                  ) : (
                    sortedPayrolls.map((payroll) => (
                      <tr key={payroll.id}>
                        <td>{payroll.payrollNumber}</td>
                        <td>
                          {formatDate(payroll.payPeriodStart)} - {formatDate(payroll.payPeriodEnd)}
                        </td>
                        <td>{formatDate(payroll.payDate)}</td>
                        <td>{payroll.employeeCount}</td>
                        <td>{payroll.totalGross.toFixed(2)}</td>
                        <td>{payroll.totalDeductions.toFixed(2)}</td>
                        <td className="font-semibold">{payroll.totalNet.toFixed(2)}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(payroll.status)}`}>
                            {getStatusLabel(payroll.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/hrm/payrolls/${payroll.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {payroll.status === "Draft" && (
                              <button
                                onClick={() => handleProcess(payroll.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Process Payroll"
                              >
                                <i className="mgc_play_line"></i>
                              </button>
                            )}
                            {payroll.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(payroll.id || "")}
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

export default PayrollList;

