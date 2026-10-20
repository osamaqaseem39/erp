import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { PageBreadcrumb, useTableSort, SortableTh } from "../../../../../components";
import { chartOfAccountService, type ChartOfAccountDto } from "../services";

/** Posting / leaf accounts (Account Information) for voucher debit pickers. */
const AccountInformationList: React.FC = () => {
  const [accounts, setAccounts] = useState<ChartOfAccountDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [q, setQ] = useState("");

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const data = await chartOfAccountService.getAll({ postingOnly: true, isActive: true });
        setAccounts(data);
      } catch (err: any) {
        setError(err.response?.data?.Error || err.message || "Failed to load");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const filtered = useMemo(() => {
    const term = q.trim().toLowerCase();
    if (!term) return accounts;
    return accounts.filter(
      (a) =>
        a.accountCode?.toLowerCase().includes(term) ||
        a.accountName?.toLowerCase().includes(term) ||
        a.remarks?.toLowerCase().includes(term)
    );
  }, [accounts, q]);

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("accountCode");
  const sorted = useMemo(() => sortRows(filtered), [filtered, sortRows]);

  return (
    <>
      <PageBreadcrumb title="Account Information" subName="Finance" />
      <div className="card">
        <div className="card-header flex items-center justify-between">
          <h4 className="card-title">Account Information (posting leaves)</h4>
          <Link to="/finance/chart-of-accounts/new" className="btn btn-primary">Add account</Link>
        </div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          <input
            className="form-input mb-3 max-w-md"
            placeholder="Search code, name, remarks…"
            value={q}
            onChange={(e) => setQ(e.target.value)}
          />
          {loading ? (
            <p>Loading…</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="table">
                <thead>
                  <tr>
                    <SortableTh label="Code" sortKey="accountCode" currentKey={sortKey} dir={sortDir} onSort={toggleSort} />
                    <SortableTh label="Name" sortKey="accountName" currentKey={sortKey} dir={sortDir} onSort={toggleSort} />
                    <th>Type</th>
                    <th>Level</th>
                    <th>Parent</th>
                    <th>Remarks</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {sorted.map((a) => (
                    <tr key={a.accountId}>
                      <td>{a.accountCode}</td>
                      <td>{a.accountName}</td>
                      <td>{a.accountType}</td>
                      <td>{a.level}</td>
                      <td>{a.parentAccountName || "—"}</td>
                      <td>{a.remarks || "—"}</td>
                      <td>
                        <Link to={`/finance/chart-of-accounts/${a.accountId}/edit`} className="btn btn-sm btn-soft-primary">
                          Edit
                        </Link>
                      </td>
                    </tr>
                  ))}
                  {sorted.length === 0 && (
                    <tr><td colSpan={7} className="text-center text-muted">No posting accounts yet.</td></tr>
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

export default AccountInformationList;
