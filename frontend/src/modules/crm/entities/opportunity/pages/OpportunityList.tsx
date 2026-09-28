import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { opportunityService, type OpportunityDto } from "../services";
import { OPPORTUNITY_STAGE_OPTIONS, OPPORTUNITY_STATUS_OPTIONS } from "../types";

const OpportunityList: React.FC = () => {
  const [opportunities, setOpportunities] = useState<OpportunityDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");
  const [filterStage, setFilterStage] = useState<string>("");

  useEffect(() => {
    loadOpportunities();
  }, [filterStatus, filterStage]);

  const loadOpportunities = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: OpportunityDto[];
      if (filterStatus) {
        data = await opportunityService.getByStatus(filterStatus);
      } else if (filterStage) {
        data = await opportunityService.getByStage(filterStage);
      } else {
        data = await opportunityService.getAll();
      }
      setOpportunities(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load opportunities");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this opportunity?")) return;

    try {
      await opportunityService.delete(id);
      loadOpportunities();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete opportunity");
    }
  };

  const handleConvert = async (id: string) => {
    if (!window.confirm("Are you sure you want to convert this opportunity to a quote?")) return;

    try {
      await opportunityService.convertToQuote(id);
      alert("Opportunity converted to quote successfully!");
      loadOpportunities();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to convert opportunity");
    }
  };

  const getStageLabel = (stage?: string) => {
    const option = OPPORTUNITY_STAGE_OPTIONS.find((opt) => opt.value === stage);
    return option?.label || stage || "N/A";
  };

  const getStatusLabel = (status?: string) => {
    const option = OPPORTUNITY_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Won":
        return "bg-success";
      case "Lost":
        return "bg-danger";
      case "Open":
        return "bg-primary";
      case "On Hold":
        return "bg-warning";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("title");
  const sortedOpportunities = useMemo(
    () => sortRows(opportunities),
    [opportunities, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Opportunities" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Opportunities</h4>
            <Link to="/crm/opportunities/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Opportunity
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2">
            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {OPPORTUNITY_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              className="form-select"
              value={filterStage}
              onChange={(e) => setFilterStage(e.target.value)}
            >
              <option value="">All Stages</option>
              {OPPORTUNITY_STAGE_OPTIONS.map((option) => (
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
                <SortableTh columnKey="opportunityCode" label="Opportunity Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="title" label="Title" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="stage" label="Stage" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="amount" label="Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="probability" label="Probability" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="expectedCloseDate" label="Expected Close Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="assignedToUserName" label="Assigned To" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {opportunities.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="text-center py-8">
                        No opportunities found
                      </td>
                    </tr>
                  ) : (
                    sortedOpportunities.map((opportunity) => (
                      <tr key={opportunity.id}>
                        <td>{opportunity.opportunityCode || "-"}</td>
                        <td>{opportunity.title || opportunity.opportunityName || "-"}</td>
                        <td>{opportunity.customerName || "-"}</td>
                        <td>
                          <span className="badge bg-info">{getStageLabel(opportunity.stage)}</span>
                        </td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(opportunity.status)}`}>
                            {getStatusLabel(opportunity.status)}
                          </span>
                        </td>
                        <td>
                          {opportunity.amount || opportunity.estimatedValue
                            ? (opportunity.amount || opportunity.estimatedValue || 0).toFixed(2)
                            : "-"}
                        </td>
                        <td>{opportunity.probability ? `${opportunity.probability}%` : "-"}</td>
                        <td>{formatDate(opportunity.expectedCloseDate)}</td>
                        <td>{opportunity.assignedToUserName || opportunity.assignedToName || "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/opportunities/${opportunity.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {opportunity.status === "Open" && (
                              <button
                                onClick={() => handleConvert(opportunity.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Convert to Quote"
                              >
                                <i className="mgc_file_list_line"></i>
                              </button>
                            )}
                            {opportunity.status === "Open" && (
                              <button
                                onClick={() => handleDelete(opportunity.id || "")}
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

export default OpportunityList;

