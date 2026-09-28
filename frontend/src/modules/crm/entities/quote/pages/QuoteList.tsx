import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { quoteService, type QuoteDto } from "../services";
import { QUOTE_STATUS_OPTIONS } from "../types";

const QuoteList: React.FC = () => {
  const [quotes, setQuotes] = useState<QuoteDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadQuotes();
  }, [filterStatus]);

  const loadQuotes = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: QuoteDto[];
      if (filterStatus) {
        data = await quoteService.getByStatus(filterStatus);
      } else {
        data = await quoteService.getAll();
      }
      setQuotes(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load quotes");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this quote?")) return;

    try {
      await quoteService.delete(id);
      loadQuotes();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete quote");
    }
  };

  const handleSend = async (id: string) => {
    if (!window.confirm("Are you sure you want to send this quote?")) return;

    try {
      await quoteService.sendQuote(id);
      alert("Quote sent successfully!");
      loadQuotes();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to send quote");
    }
  };

  const handleConvert = async (id: string) => {
    if (!window.confirm("Are you sure you want to convert this quote to a sales order?")) return;

    try {
      await quoteService.convertToSalesOrder(id);
      alert("Quote converted to sales order successfully!");
      loadQuotes();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to convert quote");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = QUOTE_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Accepted":
        return "bg-success";
      case "Converted":
        return "bg-info";
      case "Sent":
        return "bg-primary";
      case "Draft":
        return "bg-secondary";
      case "Rejected":
        return "bg-danger";
      case "Expired":
        return "bg-warning";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("quoteDate");
  const sortedQuotes = useMemo(
    () => sortRows(quotes),
    [quotes, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Quotes" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Quotes</h4>
            <Link to="/crm/quotes/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Quote
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4">
            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {QUOTE_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="quoteNumber" label="Quote Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="opportunityName" label="Opportunity" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="quoteDate" label="Quote Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="validUntil" label="Valid Until" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalAmount" label="Total Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="assignedToUserName" label="Assigned To" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {quotes.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No quotes found
                      </td>
                    </tr>
                  ) : (
                    sortedQuotes.map((quote) => (
                      <tr key={quote.id}>
                        <td>{quote.quoteNumber || "-"}</td>
                        <td>{quote.customerName || "-"}</td>
                        <td>{quote.opportunityName || "-"}</td>
                        <td>{formatDate(quote.quoteDate)}</td>
                        <td>{formatDate(quote.validUntil)}</td>
                        <td>{quote.totalAmount ? quote.totalAmount.toFixed(2) : "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(quote.status)}`}>
                            {getStatusLabel(quote.status)}
                          </span>
                        </td>
                        <td>{quote.assignedToUserName || "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/quotes/${quote.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {quote.status === "Draft" && (
                              <button
                                onClick={() => handleSend(quote.id || "")}
                                className="btn btn-sm btn-soft-info"
                                title="Send Quote"
                              >
                                <i className="mgc_mail_line"></i>
                              </button>
                            )}
                            {quote.status === "Accepted" && (
                              <button
                                onClick={() => handleConvert(quote.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Convert to Sales Order"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {quote.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(quote.id || "")}
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

export default QuoteList;

