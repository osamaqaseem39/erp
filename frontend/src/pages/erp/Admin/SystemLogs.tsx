import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../components";
import { PageBreadcrumb } from "../../../components";
// Use Core module service to avoid duplication
import { systemLogService, type SystemLogDto } from "../../../modules/core/entities/system-log/services";

const SystemLogs = () => {
  const [logs, setLogs] = useState<SystemLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterLevel, setFilterLevel] = useState<string>("");

  useEffect(() => {
    loadLogs();
  }, [filterLevel]);

  const loadLogs = async () => {
    try {
      setLoading(true);
      let data: SystemLogDto[];
      if (filterLevel) {
        data = await systemLogService.getByLevel(filterLevel);
      } else {
        data = await systemLogService.getAll();
      }
      setLogs(data);
    } catch (err: any) {
      setError(err.response?.data?.message || "Failed to load system logs");
    } finally {
      setLoading(false);
    }
  };

  const levels = ['Information', 'Warning', 'Error', 'Critical'];

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getLevelBadgeClass = (level: string) => {
    switch (level.toLowerCase()) {
      case 'error':
      case 'critical':
        return 'bg-danger';
      case 'warning':
        return 'bg-warning';
      case 'information':
      case 'info':
        return 'bg-info';
      default:
        return 'bg-secondary';
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("timestamp");
  const sortedLogs = useMemo(
    () => sortRows(logs),
    [logs, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="System Logs" subName="Admin" />
      <div className="card">
        <div className="card-header">
          <h4 className="card-title">System Logs</h4>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2">
              Filter by Level
            </label>
            <select
              value={filterLevel}
              onChange={(e) => setFilterLevel(e.target.value)}
              className="form-select"
            >
              <option value="">All Levels</option>
              {levels.map((level) => (
                <option key={level} value={level}>
                  {level}
                </option>
              ))}
            </select>
          </div>

          {loading ? (
            <div className="text-center py-8">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                <tr>
                <SortableTh columnKey="timestamp" label="Timestamp" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="level" label="Level" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="message" label="Message" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="userName" label="User" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="source" label="Source" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                </tr>
              </thead>
                <tbody>
                  {logs.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="text-center p-8 text-gray-500">
                        No system logs found
                      </td>
                    </tr>
                  ) : (
                    sortedLogs.map((log) => (
                      <tr key={log.id} className="border-b border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800">
                        <td className="p-3 text-sm text-gray-500">{formatDate(log.timestamp)}</td>
                        <td className="p-3">
                          <span className={`badge ${getLevelBadgeClass(log.logLevel)}`}>
                            {log.logLevel}
                          </span>
                        </td>
                        <td className="p-3">
                          <div className="max-w-md">
                            <div className="text-sm">{log.message}</div>
                            {log.exception && (
                              <details className="mt-1">
                                <summary className="text-xs text-gray-400 cursor-pointer">Exception details</summary>
                                <pre className="text-xs text-gray-500 mt-1 whitespace-pre-wrap">{log.exception}</pre>
                              </details>
                            )}
                          </div>
                        </td>
                        <td className="p-3 text-gray-500">{log.userName || log.userId || '-'}</td>
                        <td className="p-3 text-gray-500">{log.source || '-'}</td>
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

export default SystemLogs;

