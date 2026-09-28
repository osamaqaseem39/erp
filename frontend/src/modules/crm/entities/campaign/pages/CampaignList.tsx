import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { campaignService, type CampaignDto } from "../services";
import { CAMPAIGN_TYPE_OPTIONS, CAMPAIGN_STATUS_OPTIONS } from "../types";

const CampaignList: React.FC = () => {
  const [campaigns, setCampaigns] = useState<CampaignDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");
  const [filterType, setFilterType] = useState<string>("");

  useEffect(() => {
    loadCampaigns();
  }, [filterStatus, filterType]);

  const loadCampaigns = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: CampaignDto[];
      if (filterStatus) {
        data = await campaignService.getByStatus(filterStatus);
      } else if (filterType) {
        data = await campaignService.getByType(filterType);
      } else {
        data = await campaignService.getAll();
      }
      setCampaigns(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load campaigns");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this campaign?")) return;

    try {
      await campaignService.delete(id);
      loadCampaigns();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete campaign");
    }
  };

  const handleLaunch = async (id: string) => {
    if (!window.confirm("Are you sure you want to launch this campaign?")) return;

    try {
      await campaignService.launch(id);
      alert("Campaign launched successfully!");
      loadCampaigns();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to launch campaign");
    }
  };

  const handlePause = async (id: string) => {
    if (!window.confirm("Are you sure you want to pause this campaign?")) return;

    try {
      await campaignService.pause(id);
      alert("Campaign paused successfully!");
      loadCampaigns();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to pause campaign");
    }
  };

  const handleComplete = async (id: string) => {
    if (!window.confirm("Are you sure you want to complete this campaign?")) return;

    try {
      await campaignService.complete(id);
      alert("Campaign completed successfully!");
      loadCampaigns();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to complete campaign");
    }
  };

  const getTypeLabel = (type?: string) => {
    const option = CAMPAIGN_TYPE_OPTIONS.find((opt) => opt.value === type);
    return option?.label || type || "N/A";
  };

  const getStatusLabel = (status?: string) => {
    const option = CAMPAIGN_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "Active":
        return "bg-info";
      case "Scheduled":
        return "bg-primary";
      case "Paused":
        return "bg-warning";
      case "Draft":
        return "bg-secondary";
      case "Cancelled":
        return "bg-danger";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("campaignName");
  const sortedCampaigns = useMemo(
    () => sortRows(campaigns),
    [campaigns, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Campaigns" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Campaigns</h4>
            <Link to="/crm/campaigns/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Campaign
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
              {CAMPAIGN_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              className="form-select"
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
            >
              <option value="">All Types</option>
              {CAMPAIGN_TYPE_OPTIONS.map((option) => (
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
                <SortableTh columnKey="campaignName" label="Campaign Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="campaignType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="startDate" label="Start Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="endDate" label="End Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="budget" label="Budget" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="actualCost" label="Actual Cost" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="expectedResponse" label="Expected Response" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="actualResponse" label="Actual Response" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {campaigns.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="text-center py-8">
                        No campaigns found
                      </td>
                    </tr>
                  ) : (
                    sortedCampaigns.map((campaign) => (
                      <tr key={campaign.id}>
                        <td>{campaign.campaignName || "-"}</td>
                        <td>
                          <span className="badge bg-info">{getTypeLabel(campaign.campaignType)}</span>
                        </td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(campaign.status)}`}>
                            {getStatusLabel(campaign.status)}
                          </span>
                        </td>
                        <td>{formatDate(campaign.startDate)}</td>
                        <td>{formatDate(campaign.endDate)}</td>
                        <td>{campaign.budget ? campaign.budget.toFixed(2) : "-"}</td>
                        <td>{campaign.actualCost ? campaign.actualCost.toFixed(2) : "-"}</td>
                        <td>{campaign.expectedResponse || "-"}</td>
                        <td>{campaign.actualResponse || "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/campaigns/${campaign.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {campaign.status === "Draft" && (
                              <button
                                onClick={() => handleLaunch(campaign.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Launch"
                              >
                                <i className="mgc_play_line"></i>
                              </button>
                            )}
                            {campaign.status === "Active" && (
                              <button
                                onClick={() => handlePause(campaign.id || "")}
                                className="btn btn-sm btn-soft-warning"
                                title="Pause"
                              >
                                <i className="mgc_pause_line"></i>
                              </button>
                            )}
                            {(campaign.status === "Active" || campaign.status === "Paused") && (
                              <button
                                onClick={() => handleComplete(campaign.id || "")}
                                className="btn btn-sm btn-soft-info"
                                title="Complete"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {campaign.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(campaign.id || "")}
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

export default CampaignList;

