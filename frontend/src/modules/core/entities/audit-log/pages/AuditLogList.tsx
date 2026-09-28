import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { PageBreadcrumb } from "../../../../../components";
import { auditLogService, type AuditLogDto } from "../services";

const AuditLogList: React.FC = () => {
  const [logs, setLogs] = useState<AuditLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadLogs();
  }, []);

  const loadLogs = async () => {
    try {
      setLoading(true);
      const data = await auditLogService.getAll();
      setLogs(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load audit logs");
    } finally {
      setLoading(false);
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("timestamp");
  const sortedLogs = useMemo(
    () => sortRows(logs),
    [logs, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Audit Logs" subName="Core" />
      <div className="card">
        <div className="card-header">
          <h4 className="card-title">Audit Logs</h4>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

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
                <SortableTh columnKey="timestamp" label="Timestamp" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="entityName" label="Entity" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="action" label="Action" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="userName" label="User" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="details" label="Details" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                </tr>
              </thead>
                <tbody>
                  {logs.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="text-center py-8">
                        No audit logs found
                      </td>
                    </tr>
                  ) : (
                    sortedLogs.map((log) => (
                      <tr key={log.id}>
                        <td>{new Date(log.timestamp).toLocaleString()}</td>
                        <td>{log.entityName}</td>
                        <td>
                          <span className="badge bg-info">{log.action}</span>
                        </td>
                        <td>{log.userName || log.userId || "-"}</td>
                        <td>
                          <button
                            className="btn btn-sm btn-soft-primary"
                            onClick={() => {
                              // Show details in modal or expand row
                              console.log("Log details:", log);
                            }}
                          >
                            View
                          </button>
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

export default AuditLogList;

