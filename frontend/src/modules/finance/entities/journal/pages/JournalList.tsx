import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { journalService, type JournalDto } from "../services";
import { JOURNAL_TYPE_OPTIONS, JOURNAL_STATUS_OPTIONS } from "../types";

const JournalList: React.FC = () => {
  const [journals, setJournals] = useState<JournalDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterType, setFilterType] = useState<string>("");
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadJournals();
  }, [filterType, filterStatus]);

  const loadJournals = async () => {
    setLoading(true);
    setError(null);
    try {
      const query: any = {};
      if (filterType) query.journalType = filterType;
      if (filterStatus) query.status = filterStatus;
      
      const data = await journalService.getAll(query);
      setJournals(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load journals");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this journal?")) return;

    try {
      await journalService.delete(id);
      loadJournals();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete journal");
    }
  };

  const handlePost = async (id: string) => {
    if (!window.confirm("Are you sure you want to post this journal?")) return;

    try {
      await journalService.post(id);
      loadJournals();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to post journal");
    }
  };

  const getTypeLabel = (type?: string) => {
    const option = JOURNAL_TYPE_OPTIONS.find((opt) => opt.value === type);
    return option?.label || type || "N/A";
  };

  const getStatusLabel = (status?: string) => {
    const option = JOURNAL_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("journalDate");
  const sortedJournals = useMemo(
    () => sortRows(journals),
    [journals, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Journals" subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Journals</h4>
            <Link to="/finance/journals/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Journal
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-3">
            <select
              className="form-select"
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
            >
              <option value="">All Types</option>
              {JOURNAL_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {JOURNAL_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="journalId" label="Journal ID" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="journalType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="referenceNo" label="Reference No" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="journalDate" label="Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalDebit" label="Total Debit" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalCredit" label="Total Credit" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {journals.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-8">
                        No journals found
                      </td>
                    </tr>
                  ) : (
                    sortedJournals.map((journal) => (
                      <tr key={journal.journalId || journal.id}>
                        <td>{journal.journalId || journal.id}</td>
                        <td>
                          <span className="badge bg-info">{getTypeLabel(journal.journalType)}</span>
                        </td>
                        <td>{journal.referenceNo || "-"}</td>
                        <td>{formatDate(journal.journalDate)}</td>
                        <td>{journal.totalDebit?.toFixed(2) || "0.00"}</td>
                        <td>{journal.totalCredit?.toFixed(2) || "0.00"}</td>
                        <td>
                          {journal.status === "Posted" ? (
                          <span className="badge badge-pill badge-soft-success">{getStatusLabel(journal.status)}</span>
                          ) : journal.status === "Draft" ? (
                            <span className="badge bg-warning">{getStatusLabel(journal.status)}</span>
                          ) : (
                            <span className="badge bg-secondary">{getStatusLabel(journal.status)}</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/finance/journals/${journal.journalId || journal.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {journal.status === "Draft" && (
                              <button
                                onClick={() => handlePost(journal.journalId || journal.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Post Journal"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {journal.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(journal.journalId || journal.id || "")}
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

export default JournalList;

