import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { chartOfAccountService, type ChartOfAccountDto } from "../services";
import { ACCOUNT_TYPE_OPTIONS } from "../types";

const ChartOfAccountList: React.FC = () => {
  const [accounts, setAccounts] = useState<ChartOfAccountDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterAccountType, setFilterAccountType] = useState<string>("");
  const [filterIsActive, setFilterIsActive] = useState<boolean | undefined>(undefined);

  useEffect(() => {
    loadAccounts();
  }, [filterAccountType, filterIsActive]);

  const loadAccounts = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await chartOfAccountService.getAll({
        accountType: filterAccountType || undefined,
        isActive: filterIsActive,
      });
      setAccounts(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load accounts");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this account?")) return;

    try {
      await chartOfAccountService.delete(id);
      loadAccounts();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete account");
    }
  };

  const getAccountTypeLabel = (accountType?: string) => {
    const option = ACCOUNT_TYPE_OPTIONS.find((opt) => opt.value === accountType);
    return option?.label || accountType || "N/A";
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("accountCode");
  const sortedAccounts = useMemo(
    () => sortRows(accounts),
    [accounts, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Chart of Accounts" subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Chart of Accounts</h4>
            <Link to="/finance/chart-of-accounts/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Account
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Account Type
              </label>
              <select
                className="form-select"
                value={filterAccountType}
                onChange={(e) => setFilterAccountType(e.target.value)}
              >
                <option value="">All Types</option>
                {ACCOUNT_TYPE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Status
              </label>
              <select
                className="form-select"
                value={filterIsActive === undefined ? "" : filterIsActive ? "true" : "false"}
                onChange={(e) =>
                  setFilterIsActive(e.target.value === "" ? undefined : e.target.value === "true")
                }
              >
                <option value="">All</option>
                <option value="true">Active</option>
                <option value="false">Inactive</option>
              </select>
            </div>
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
                <SortableTh columnKey="accountCode" label="Account Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="accountName" label="Account Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="accountType" label="Account Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="parentAccountName" label="Parent Account" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="currencyCode" label="Currency" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isPostingAllowed" label="Posting Allowed" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {accounts.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-8">
                        No accounts found
                      </td>
                    </tr>
                  ) : (
                    sortedAccounts.map((account) => (
                      <tr key={account.accountId || account.id}>
                        <td>{account.accountCode || "N/A"}</td>
                        <td>{account.accountName || "N/A"}</td>
                        <td>
                          <span className="badge bg-info">{getAccountTypeLabel(account.accountType)}</span>
                        </td>
                        <td>{account.parentAccountName || "Root"}</td>
                        <td>{account.currencyCode || "N/A"}</td>
                        <td>
                          {account.isPostingAllowed ? (
                          <span className="badge badge-pill badge-soft-success">Yes</span>
                          ) : (
                            <span className="badge bg-secondary">No</span>
                          )}
                        </td>
                        <td>
                          {account.isActive ? (
                          <span className="badge badge-pill badge-soft-success">Active</span>
                          ) : (
                            <span className="badge bg-danger">Inactive</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/finance/chart-of-accounts/${account.accountId || account.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(account.accountId || account.id || "")}
                              className="btn btn-sm btn-soft-danger"
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

export default ChartOfAccountList;

