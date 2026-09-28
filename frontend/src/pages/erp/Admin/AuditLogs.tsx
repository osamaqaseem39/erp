import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../components";
import { PageBreadcrumb } from "../../../components";
// Use Core module service to avoid duplication
import { auditLogService, type AuditLogDto } from "../../../modules/core/entities/audit-log/services";

const AuditLogs = () => {
  const [logs, setLogs] = useState<AuditLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterAction, setFilterAction] = useState<string>("");

  useEffect(() => {
    loadLogs();
  }, [filterAction]);

  const loadLogs = async () => {
    try {
      setLoading(true);
      let data: AuditLogDto[];
      if (filterAction) {
        data = await auditLogService.getByAction(filterAction);
      } else {
        data = await auditLogService.getAll();
      }
      setLogs(data);
    } catch (err: any) {
      setError(err.response?.data?.message || "Failed to load audit logs");
    } finally {
      setLoading(false);
    }
  };

  const actions = Array.from(new Set(sortedLogs.map(l => l.action).filter(Boolean)));

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("timestamp");
  const sortedLogs = useMemo(
    () => sortRows(logs),
    [logs, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Audit Logs" subName="Admin" />
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

          {actions.length > 0 && (
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2">
                Filter by Action
              </label>
              <select
                value={filterAction}
                onChange={(e) => setFilterAction(e.target.value)}
                className="form-select"
              >
                <option value="">All Actions</option>
                {actions.map((action) => (
                  <option key={action} value={action}>
                    {action}
                  </option>
                ))}
              </select>
            </div>
          )}

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
                <SortableTh columnKey="userName" label="User" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="entityName" label="Entity" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="action" label="Action" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="ipAddress" label="IP Address" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                </tr>
              </thead>
                <tbody>
                  {logs.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="text-center p-8 text-gray-500">
                        No audit logs found
                      </td>
                    </tr>
                  ) : (
                    logs.map((log) => (
                      <tr key={log.id} className="border-b border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800">
                        <td className="p-3 text-sm text-gray-500">{formatDate(log.timestamp)}</td>
                        <td className="p-3">{log.userName || log.userId || '-'}</td>
                        <td className="p-3">
                          <span className="font-medium">{log.entityName}</span>
                          <span className="text-gray-400 ms-2">({log.entityId})</span>
                        </td>
                        <td className="p-3">
                          <span className={`badge ${
                            log.action === 'Create' ? 'bg-success' :
                            log.action === 'Update' ? 'bg-warning' :
                            log.action === 'Delete' ? 'bg-danger' :
                            'bg-info'
                          }`}>
                            {log.action}
                          </span>
                        </td>
                        <td className="p-3 text-gray-500">{log.ipAddress || '-'}</td>
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

export default AuditLogs;

