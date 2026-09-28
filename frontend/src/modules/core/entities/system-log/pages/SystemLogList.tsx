import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { PageBreadcrumb } from "../../../../../components";
import { systemLogService, type SystemLogDto } from "../services";

const SystemLogList: React.FC = () => {
  const [logs, setLogs] = useState<SystemLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [logLevel, setLogLevel] = useState<string>("");

  useEffect(() => {
    loadLogs();
  }, [logLevel]);

  const loadLogs = async () => {
    try {
      setLoading(true);
      const data = logLevel
        ? await systemLogService.getByLevel(logLevel)
        : await systemLogService.getAll();
      setLogs(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load system logs");
    } finally {
      setLoading(false);
    }
  };

  const getLogLevelBadge = (level: string) => {
    const badges: Record<string, string> = {
      Error: "bg-danger",
      Warning: "bg-warning",
      Information: "bg-info",
      Debug: "bg-secondary",
    };
    return badges[level] || "bg-secondary";
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("timestamp");
  const sortedLogs = useMemo(
    () => sortRows(logs),
    [logs, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="System Logs" subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">System Logs</h4>
            <select
              value={logLevel}
              onChange={(e) => setLogLevel(e.target.value)}
              className="form-select"
              style={{ width: "auto" }}
            >
              <option value="">All Levels</option>
              <option value="Error">Error</option>
              <option value="Warning">Warning</option>
              <option value="Information">Information</option>
              <option value="Debug">Debug</option>
            </select>
          </div>
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
                <SortableTh columnKey="level" label="Level" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="message" label="Message" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="userName" label="User" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="source" label="Source" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                </tr>
              </thead>
                <tbody>
                  {logs.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="text-center py-8">
                        No system logs found
                      </td>
                    </tr>
                  ) : (
                    sortedLogs.map((log) => (
                      <tr key={log.id}>
                        <td>{new Date(log.timestamp).toLocaleString()}</td>
                        <td>
                          <span className={`badge ${getLogLevelBadge(log.logLevel)}`}>
                            {log.logLevel}
                          </span>
                        </td>
                        <td>{log.message}</td>
                        <td>{log.userName || log.userId || "-"}</td>
                        <td>{log.source || "-"}</td>
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

export default SystemLogList;

