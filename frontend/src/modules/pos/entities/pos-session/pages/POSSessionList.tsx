import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { posSessionService, type POSSessionDto } from "../services";
import Swal from "sweetalert2";

const POSSessionList: React.FC = () => {
  const [sessions, setSessions] = useState<POSSessionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>("");

  useEffect(() => {
    loadSessions();
  }, [statusFilter]);

  const loadSessions = async () => {
    try {
      setLoading(true);
      const data = await posSessionService.getAll(undefined, statusFilter || undefined);
      setSessions(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load POS sessions");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, sessionNumber: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete POS session ${sessionNumber}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await posSessionService.delete(id!);
        Swal.fire("Deleted!", "POS session has been deleted.", "success");
        loadSessions();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete POS session", "error");
      }
    }
  };

  const handleCloseSession = async (session: POSSessionDto) => {
    const { value: closingBalance } = await Swal.fire({
      title: "Close POS Session",
      text: `Enter closing balance for session ${session.sessionNumber}`,
      input: "number",
      inputLabel: "Closing Balance",
      inputValue: session.expectedBalance.toString(),
      inputAttributes: {
        step: "0.01",
        min: "0",
      },
      showCancelButton: true,
      confirmButtonText: "Close Session",
      inputValidator: (value) => {
        if (!value) {
          return "Closing balance is required";
        }
        if (parseFloat(value) < 0) {
          return "Closing balance must be >= 0";
        }
      },
    });

    if (closingBalance) {
      try {
        await posSessionService.closeSession(session.id!, {
          sessionId: session.id!,
          closingBalance: parseFloat(closingBalance),
        });
        Swal.fire("Closed!", "POS session has been closed.", "success");
        loadSessions();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to close POS session", "error");
      }
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("startTime");
  const sortedSessions = useMemo(
    () => sortRows(sessions),
    [sessions, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="POS Sessions" subName="POS" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">POS Sessions</h4>
            <Link to="/pos/sessions/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>New Session
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {/* Filters */}
          <div className="mb-4 flex items-center gap-4">
            <div>
              <select
                className="form-select"
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="">All Status</option>
                <option value="Open">Open</option>
                <option value="Closed">Closed</option>
                <option value="Suspended">Suspended</option>
              </select>
            </div>
          </div>

          {loading ? (
            <div className="text-center py-8">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                <tr>
                <SortableTh columnKey="sessionNumber" label="Session Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="cashierName" label="Cashier" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="terminalName" label="Terminal" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="startTime" label="Start Time" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="endTime" label="End Time" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="openingBalance" label="Opening Balance" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="expectedBalance" label="Expected Balance" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="closingBalance" label="Closing Balance" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="difference" label="Difference" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {sessions.length === 0 ? (
                    <tr>
                      <td colSpan={11} className="text-center py-8">
                        No POS sessions found
                      </td>
                    </tr>
                  ) : (
                    sortedSessions.map((session) => (
                      <tr key={session.id}>
                        <td>{session.sessionNumber}</td>
                        <td>{session.cashierName || "-"}</td>
                        <td>{session.terminalName || session.terminalId}</td>
                        <td>{session.startTime ? new Date(session.startTime).toLocaleString() : "-"}</td>
                        <td>{session.endTime ? new Date(session.endTime).toLocaleString() : "-"}</td>
                        <td>{session.openingBalance.toFixed(2)}</td>
                        <td>{session.expectedBalance.toFixed(2)}</td>
                        <td>{session.closingBalance.toFixed(2)}</td>
                        <td>
                          <span className={session.difference !== 0 ? "text-danger" : "text-success"}>
                            {session.difference.toFixed(2)}
                          </span>
                        </td>
                        <td>
                          <span className={`badge ${
                            session.status === "Open" ? "bg-success" :
                            session.status === "Closed" ? "bg-secondary" :
                            "bg-warning"
                          }`}>
                            {session.status}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/pos/sessions/${session.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                              title="Edit"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {session.status === "Open" && (
                              <button
                                onClick={() => handleCloseSession(session)}
                                className="btn btn-sm btn-soft-warning"
                                title="Close Session"
                              >
                                <i className="mgc_close_line"></i>
                              </button>
                            )}
                            <button
                              onClick={() => handleDelete(session.id!, session.sessionNumber)}
                              className="btn btn-sm btn-soft-danger"
                              title="Delete"
                            >
                              <i className="mgc_delete_line"></i>
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
        </div>
      </div>
    </>
  );
};

export default POSSessionList;

