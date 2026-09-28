import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { leaveRequestService, type LeaveRequestDto } from "../services";
import { LEAVE_REQUEST_STATUS_OPTIONS } from "../types";

const LeaveRequestList: React.FC = () => {
  const [leaveRequests, setLeaveRequests] = useState<LeaveRequestDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadLeaveRequests();
  }, [filterStatus]);

  const loadLeaveRequests = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: LeaveRequestDto[];
      if (filterStatus) {
        data = await leaveRequestService.getByStatus(filterStatus);
      } else {
        data = await leaveRequestService.getAll();
      }
      setLeaveRequests(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load leave requests");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this leave request?")) return;

    try {
      await leaveRequestService.delete(id);
      loadLeaveRequests();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete leave request");
    }
  };

  const handleApprove = async (id: string) => {
    if (!window.confirm("Are you sure you want to approve this leave request?")) return;

    try {
      await leaveRequestService.approve(id);
      loadLeaveRequests();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to approve leave request");
    }
  };

  const handleReject = async (id: string) => {
    const reason = window.prompt("Please provide a reason for rejection:");
    if (reason === null) return;

    try {
      await leaveRequestService.reject(id, reason);
      loadLeaveRequests();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to reject leave request");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = LEAVE_REQUEST_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Approved":
        return "bg-success";
      case "Rejected":
        return "bg-danger";
      case "Pending":
        return "bg-warning";
      case "Cancelled":
        return "bg-secondary";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("startDate");
  const sortedLeaveRequests = useMemo(
    () => sortRows(leaveRequests),
    [leaveRequests, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Leave Requests" subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Leave Requests</h4>
            <Link to="/hrm/leave-requests/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Leave Request
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
              {LEAVE_REQUEST_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="leaveType" label="Leave Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="startDate" label="Start Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="endDate" label="End Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="days" label="Days" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="appliedAt" label="Applied At" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {leaveRequests.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-8">
                        No leave requests found
                      </td>
                    </tr>
                  ) : (
                    sortedLeaveRequests.map((leaveRequest) => (
                      <tr key={leaveRequest.id}>
                        <td>{leaveRequest.employeeName || leaveRequest.employeeId}</td>
                        <td>{leaveRequest.leaveTypeName || "-"}</td>
                        <td>{formatDate(leaveRequest.startDate)}</td>
                        <td>{formatDate(leaveRequest.endDate)}</td>
                        <td>{leaveRequest.daysRequested}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(leaveRequest.status)}`}>
                            {getStatusLabel(leaveRequest.status)}
                          </span>
                        </td>
                        <td>{formatDate(leaveRequest.appliedAt)}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/hrm/leave-requests/${leaveRequest.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {leaveRequest.status === "Pending" && (
                              <>
                                <button
                                  onClick={() => handleApprove(leaveRequest.id || "")}
                                  className="btn btn-sm btn-soft-success"
                                  title="Approve"
                                >
                                  <i className="mgc_check_line"></i>
                                </button>
                                <button
                                  onClick={() => handleReject(leaveRequest.id || "")}
                                  className="btn btn-sm btn-soft-danger"
                                  title="Reject"
                                >
                                  <i className="mgc_close_line"></i>
                                </button>
                              </>
                            )}
                            {leaveRequest.status === "Pending" && (
                              <button
                                onClick={() => handleDelete(leaveRequest.id || "")}
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

export default LeaveRequestList;

