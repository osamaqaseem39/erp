import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { PageBreadcrumb, useTableSort, SortableTh } from "../../../../../components";
import { chartOfAccountService, type ChartOfAccountDto } from "../services";
import { ACCOUNT_TYPE_OPTIONS } from "../types";

const levelTitles: Record<number, string> = {
  1: "First Level Accounts",
  2: "Second Level Accounts",
  3: "Third Level Accounts",
  4: "Fourth Level Accounts",
};

const AccountLevelList: React.FC = () => {
  const { level: levelParam } = useParams<{ level: string }>();
  const level = Math.min(4, Math.max(1, parseInt(levelParam || "1", 10) || 1));
  const [accounts, setAccounts] = useState<ChartOfAccountDto[]>([]);
  const [parents, setParents] = useState<ChartOfAccountDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    accountCode: "",
    accountName: "",
    accountType: "Asset",
    parentAccountId: "",
    remarks: "",
    isPostingAllowed: level >= 4,
  });

  useEffect(() => {
    load();
  }, [level]);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await chartOfAccountService.getAll({ level, isActive: true });
      setAccounts(data);
      if (level > 1) {
        const p = await chartOfAccountService.getAll({ level: level - 1, isActive: true });
        setParents(p);
      } else {
        setParents([]);
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.message || "Failed to load accounts");
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await chartOfAccountService.create({
        accountCode: form.accountCode,
        accountName: form.accountName,
        accountType: form.accountType,
        parentAccountId: form.parentAccountId || undefined,
        remarks: form.remarks || undefined,
        isActive: true,
        isPostingAllowed: form.isPostingAllowed,
      });
      setShowForm(false);
      setForm({
        accountCode: "",
        accountName: "",
        accountType: "Asset",
        parentAccountId: "",
        remarks: "",
        isPostingAllowed: level >= 4,
      });
      load();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.message || "Failed to create");
    }
  };

  const handleSeed = async () => {
    try {
      const n = await chartOfAccountService.seedDefaults();
      alert(n > 0 ? `Seeded ${n} level-1 groups.` : "CoA already has accounts.");
      load();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.message || "Seed failed");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("accountCode");
  const sorted = useMemo(() => sortRows(accounts), [accounts, sortRows]);

  return (
    <>
      <PageBreadcrumb title={levelTitles[level]} subName="Finance" />
      <div className="card">
        <div className="card-header flex items-center justify-between">
          <h4 className="card-title">{levelTitles[level]}</h4>
          <div className="flex gap-2">
            {level === 1 && (
              <button type="button" className="btn btn-soft-secondary" onClick={handleSeed}>
                Seed L1 groups
              </button>
            )}
            <button type="button" className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
              Add level {level}
            </button>
          </div>
        </div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          {showForm && (
            <form onSubmit={handleCreate} className="mb-4 grid grid-cols-1 gap-3 md:grid-cols-2 border p-3 rounded">
              {level > 1 && (
                <div>
                  <label className="form-label">Parent (level {level - 1})</label>
                  <select
                    className="form-select"
                    required
                    value={form.parentAccountId}
                    onChange={(e) => setForm({ ...form, parentAccountId: e.target.value })}
                  >
                    <option value="">Select parent</option>
                    {parents.map((p) => (
                      <option key={p.accountId} value={p.accountId}>
                        {p.accountCode} — {p.accountName}
                      </option>
                    ))}
                  </select>
                </div>
              )}
              <div>
                <label className="form-label">Code</label>
                <input className="form-input" required value={form.accountCode} onChange={(e) => setForm({ ...form, accountCode: e.target.value })} />
              </div>
              <div>
                <label className="form-label">Description</label>
                <input className="form-input" required value={form.accountName} onChange={(e) => setForm({ ...form, accountName: e.target.value })} />
              </div>
              <div>
                <label className="form-label">Type</label>
                <select className="form-select" value={form.accountType} onChange={(e) => setForm({ ...form, accountType: e.target.value })}>
                  {ACCOUNT_TYPE_OPTIONS.map((o) => (
                    <option key={o.value} value={o.value}>{o.label}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="form-label">Remarks</label>
                <input className="form-input" value={form.remarks} onChange={(e) => setForm({ ...form, remarks: e.target.value })} />
              </div>
              <div className="md:col-span-2">
                <button type="submit" className="btn btn-primary">Save</button>
              </div>
            </form>
          )}
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
                      <td>{a.remarks || "—"}</td>
                      <td>
                        <Link to={`/finance/chart-of-accounts/${a.accountId}/edit`} className="btn btn-sm btn-soft-primary">
                          Edit
                        </Link>
                      </td>
                    </tr>
                  ))}
                  {sorted.length === 0 && (
                    <tr><td colSpan={6} className="text-center text-muted">No accounts at this level.</td></tr>
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

export default AccountLevelList;
