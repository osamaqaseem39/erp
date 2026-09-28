import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { timeTrackingService, type TimeEntryDto } from "../services";
import { TIME_ENTRY_STATUS_OPTIONS } from "../types";

const TimeEntryList: React.FC = () => {
  const [timeEntries, setTimeEntries] = useState<TimeEntryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadTimeEntries();
  }, [filterStatus]);

  const loadTimeEntries = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await timeTrackingService.getAll();
      let filtered = data;
      if (filterStatus) {
        filtered = data.filter((entry) => entry.status === filterStatus);
      }
      setTimeEntries(filtered);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load time entries");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this time entry?")) return;

    try {
      await timeTrackingService.delete(id);
      loadTimeEntries();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete time entry");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = TIME_ENTRY_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Approved":
        return "bg-success";
      case "Billed":
        return "bg-info";
      case "Submitted":
        return "bg-primary";
      case "Draft":
        return "bg-secondary";
      case "Rejected":
        return "bg-danger";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const formatDateTime = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("workDate");
  const sortedTimeEntries = useMemo(
    () => sortRows(timeEntries),
    [timeEntries, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Time Entries" subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Time Entries</h4>
            <Link to="/hrm/time-entries/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Time Entry
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
              {TIME_ENTRY_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="employeeName" label="Employee" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="workDate" label="Work Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="startTime" label="Start Time" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="endTime" label="End Time" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="hoursWorked" label="Hours Worked" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="projectName" label="Project" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isBillable" label="Billable" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalAmount" label="Total Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {timeEntries.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="text-center py-8">
                        No time entries found
                      </td>
                    </tr>
                  ) : (
                    sortedTimeEntries.map((entry) => (
                      <tr key={entry.id}>
                        <td>{entry.employeeName || entry.employeeId}</td>
                        <td>{formatDate(entry.workDate)}</td>
                        <td>{formatDateTime(entry.startTime)}</td>
                        <td>{formatDateTime(entry.endTime)}</td>
                        <td>{entry.hoursWorked.toFixed(2)}</td>
                        <td>{entry.projectName || "-"}</td>
                        <td>
                          {entry.isBillable ? (
                          <span className="badge badge-pill badge-soft-success">Yes</span>
                          ) : (
                            <span className="badge bg-secondary">No</span>
                          )}
                        </td>
                        <td>{entry.totalAmount ? entry.totalAmount.toFixed(2) : "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(entry.status)}`}>
                            {getStatusLabel(entry.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/hrm/time-entries/${entry.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {entry.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(entry.id || "")}
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

export default TimeEntryList;

