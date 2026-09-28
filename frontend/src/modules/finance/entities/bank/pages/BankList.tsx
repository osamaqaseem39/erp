import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { bankService, type BankDto } from "../services";

const BankList: React.FC = () => {
  const [banks, setBanks] = useState<BankDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterIsActive, setFilterIsActive] = useState<boolean | undefined>(undefined);

  useEffect(() => {
    loadBanks();
  }, [filterIsActive]);

  const loadBanks = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await bankService.getAll();
      let filtered = data;
      if (filterIsActive !== undefined) {
        filtered = data.filter((bank) => bank.isActive === filterIsActive);
      }
      setBanks(filtered);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load banks");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this bank?")) return;

    try {
      await bankService.delete(id);
      loadBanks();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete bank");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("bankName");
  const sortedBanks = useMemo(
    () => sortRows(banks),
    [banks, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Banks" subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Banks</h4>
            <Link to="/finance/banks/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Bank
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4">
            <select
              className="form-select"
              value={filterIsActive === undefined ? "" : filterIsActive ? "true" : "false"}
              onChange={(e) =>
                setFilterIsActive(e.target.value === "" ? undefined : e.target.value === "true")
              }
            >
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
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
                <SortableTh columnKey="bankName" label="Bank Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="bankCode" label="Bank Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="swiftCode" label="SWIFT Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="city" label="City" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="country" label="Country" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="phone" label="Phone" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="email" label="Email" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {banks.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No banks found
                      </td>
                    </tr>
                  ) : (
                    sortedBanks.map((bank) => (
                      <tr key={bank.id}>
                        <td>{bank.bankName}</td>
                        <td>{bank.bankCode}</td>
                        <td>{bank.swiftCode || "-"}</td>
                        <td>{bank.city || "-"}</td>
                        <td>{bank.country || "-"}</td>
                        <td>{bank.phone || "-"}</td>
                        <td>{bank.email || "-"}</td>
                        <td>
                          {bank.isActive ? (
                          <span className="badge badge-pill badge-soft-success">Active</span>
                          ) : (
                            <span className="badge bg-danger">Inactive</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/finance/banks/${bank.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(bank.id || "")}
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

export default BankList;

