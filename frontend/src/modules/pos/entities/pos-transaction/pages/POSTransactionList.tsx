import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { posTransactionService, type POSTransactionDto } from "../services";
import Swal from "sweetalert2";

const POSTransactionList: React.FC = () => {
  const [transactions, setTransactions] = useState<POSTransactionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sessionFilter, setSessionFilter] = useState<string>("");
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");

  useEffect(() => {
    loadTransactions();
  }, [sessionFilter, startDate, endDate]);

  const loadTransactions = async () => {
    try {
      setLoading(true);
      const data = await posTransactionService.getAll(
        sessionFilter || undefined,
        undefined,
        startDate || undefined,
        endDate || undefined
      );
      setTransactions(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load POS transactions");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, transactionNumber: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete POS transaction ${transactionNumber}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await posTransactionService.delete(id!);
        Swal.fire("Deleted!", "POS transaction has been deleted.", "success");
        loadTransactions();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete POS transaction", "error");
      }
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("transactionDate");
  const sortedTransactions = useMemo(
    () => sortRows(transactions),
    [transactions, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="POS Transactions" subName="POS" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">POS Transactions</h4>
            <Link to="/pos/transactions/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>New Transaction
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {/* Filters */}
          <div className="mb-4 flex items-center gap-4">
            <div>
              <input
                type="text"
                className="form-input"
                placeholder="Session ID (GUID)"
                value={sessionFilter}
                onChange={(e) => setSessionFilter(e.target.value)}
              />
            </div>
            <div>
              <input
                type="date"
                className="form-input"
                placeholder="Start Date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </div>
            <div>
              <input
                type="date"
                className="form-input"
                placeholder="End Date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
              />
            </div>
            <button onClick={loadTransactions} className="btn btn-soft-primary">
              <i className="mgc_search_line me-2"></i>Filter
            </button>
          </div>

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
                <SortableTh columnKey="transactionNumber" label="Transaction Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="transactionDate" label="Transaction Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="paymentMethod" label="Payment Method" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="subtotal" label="Subtotal" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="tax" label="Tax" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="discount" label="Discount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="total" label="Total" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="amountPaid" label="Amount Paid" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="change" label="Change" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {transactions.length === 0 ? (
                    <tr>
                      <td colSpan={12} className="text-center py-8">
                        No POS transactions found
                      </td>
                    </tr>
                  ) : (
                    sortedTransactions.map((transaction) => (
                      <tr key={transaction.id}>
                        <td>{transaction.transactionNumber}</td>
                        <td>{transaction.customerName || "-"}</td>
                        <td>{transaction.transactionDate ? new Date(transaction.transactionDate).toLocaleString() : "-"}</td>
                        <td>{transaction.paymentMethod}</td>
                        <td>{transaction.subTotal.toFixed(2)}</td>
                        <td>{transaction.taxAmount.toFixed(2)}</td>
                        <td>{transaction.discountAmount.toFixed(2)}</td>
                        <td className="font-semibold">{transaction.totalAmount.toFixed(2)}</td>
                        <td>{transaction.amountPaid.toFixed(2)}</td>
                        <td>{transaction.changeAmount.toFixed(2)}</td>
                        <td>
                          <span className={`badge ${
                            transaction.status === "Completed" ? "bg-success" :
                            transaction.status === "Pending" ? "bg-warning" :
                            transaction.status === "Cancelled" ? "bg-danger" :
                            "bg-secondary"
                          }`}>
                            {transaction.status}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/pos/transactions/${transaction.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                              title="Edit"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(transaction.id!, transaction.transactionNumber)}
                              className="btn btn-sm btn-soft-danger"
                              title="Delete"
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

export default POSTransactionList;

