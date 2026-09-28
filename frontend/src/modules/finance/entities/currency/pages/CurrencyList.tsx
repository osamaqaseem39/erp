import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { currencyService, type CurrencyDto } from "../services";

const CurrencyList: React.FC = () => {
  const [currencies, setCurrencies] = useState<CurrencyDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCurrencies();
  }, []);

  const loadCurrencies = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await currencyService.getAll();
      setCurrencies(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load currencies");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (currencyCode: string) => {
    if (!window.confirm(`Are you sure you want to delete currency ${currencyCode}?`)) return;

    try {
      await currencyService.delete(currencyCode);
      loadCurrencies();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete currency");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("currencyCode");
  const sortedCurrencies = useMemo(
    () => sortRows(currencies),
    [currencies, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Currencies" subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Currencies</h4>
            <Link to="/finance/currencies/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Currency
            </Link>
          </div>
        </div>
        <div className="card-body">
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
                <SortableTh columnKey="currencyCode" label="Currency Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="currencyName" label="Currency Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="iso4217Code" label="ISO 4217" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="symbol" label="Symbol" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="decimalPlaces" label="Decimal Places" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isBaseCurrency" label="Base Currency" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {currencies.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-8">
                        No currencies found
                      </td>
                    </tr>
                  ) : (
                    sortedCurrencies.map((currency) => (
                      <tr key={currency.currencyCode}>
                        <td>{currency.currencyCode}</td>
                        <td>{currency.currencyName}</td>
                        <td>{currency.iso4217}</td>
                        <td>{currency.symbol}</td>
                        <td>{currency.decimalPlaces}</td>
                        <td>
                          {currency.isBaseCurrency ? (
                          <span className="badge badge-pill badge-soft-success">Yes</span>
                          ) : (
                            <span className="badge bg-secondary">No</span>
                          )}
                        </td>
                        <td>
                          {currency.isActive ? (
                          <span className="badge badge-pill badge-soft-success">Active</span>
                          ) : (
                            <span className="badge bg-danger">Inactive</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/finance/currencies/${currency.currencyCode}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(currency.currencyCode)}
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

export default CurrencyList;

