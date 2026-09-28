import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { activityService, type ActivityDto } from "../services";
import { ACTIVITY_TYPE_OPTIONS, ACTIVITY_STATUS_OPTIONS, ACTIVITY_PRIORITY_OPTIONS } from "../types";

const ActivityList: React.FC = () => {
  const [activities, setActivities] = useState<ActivityDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterType, setFilterType] = useState<string>("");
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadActivities();
  }, [filterType, filterStatus]);

  const loadActivities = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: ActivityDto[];
      if (filterType) {
        data = await activityService.getByType(filterType);
      } else if (filterStatus) {
        // Filter by status client-side since API might not have this endpoint
        const allData = await activityService.getAll();
        data = allData.filter(activity => activity.status === filterStatus);
      } else {
        data = await activityService.getAll();
      }
      setActivities(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load activities");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this activity?")) return;

    try {
      await activityService.delete(id);
      loadActivities();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete activity");
    }
  };

  const handleComplete = async (id: string) => {
    try {
      await activityService.complete(id);
      loadActivities();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to complete activity");
    }
  };

  const getTypeLabel = (type?: string) => {
    const option = ACTIVITY_TYPE_OPTIONS.find((opt) => opt.value === type);
    return option?.label || type || "N/A";
  };

  const getStatusLabel = (status?: string) => {
    const option = ACTIVITY_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "InProgress":
        return "bg-info";
      case "Open":
        return "bg-primary";
      case "Cancelled":
        return "bg-danger";
      case "Deferred":
        return "bg-warning";
      default:
        return "bg-secondary";
    }
  };

  const getPriorityBadgeClass = (priority?: string) => {
    switch (priority) {
      case "Urgent":
        return "bg-danger";
      case "High":
        return "bg-warning";
      case "Medium":
        return "bg-info";
      case "Low":
        return "bg-secondary";
      default:
        return "bg-secondary";
    }
  };

  const formatDateTime = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("startDate");
  const sortedActivities = useMemo(
    () => sortRows(activities),
    [activities, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Activities" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Activities</h4>
            <Link to="/crm/activities/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Activity
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2">
            <select
              className="form-select"
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
            >
              <option value="">All Types</option>
              {ACTIVITY_TYPE_OPTIONS.map((option) => (
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
              {ACTIVITY_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="activityType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="subject" label="Subject" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="startDate" label="Start Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="endDate" label="End Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="priority" label="Priority" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="assignedToUserName" label="Assigned To" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Related To" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {activities.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No activities found
                      </td>
                    </tr>
                  ) : (
                    sortedActivities.map((activity) => (
                      <tr key={activity.id}>
                        <td>
                          <span className="badge bg-info">{getTypeLabel(activity.activityType)}</span>
                        </td>
                        <td>{activity.subject}</td>
                        <td>{formatDateTime(activity.startDate)}</td>
                        <td>{formatDateTime(activity.endDate)}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(activity.status)}`}>
                            {getStatusLabel(activity.status)}
                          </span>
                        </td>
                        <td>
                          <span className={`badge ${getPriorityBadgeClass(activity.priority)}`}>
                            {activity.priority || "Medium"}
                          </span>
                        </td>
                        <td>{activity.assignedToUserName || activity.assignedTo || "-"}</td>
                        <td>
                          {activity.customerName && `Customer: ${activity.customerName}`}
                          {activity.leadName && `Lead: ${activity.leadName}`}
                          {activity.opportunityName && `Opportunity: ${activity.opportunityName}`}
                          {!activity.customerName && !activity.leadName && !activity.opportunityName && "-"}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/activities/${activity.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {activity.status !== "Completed" && activity.status !== "Cancelled" && (
                              <>
                                <button
                                  onClick={() => void handleComplete(activity.id || "")}
                                  className="btn btn-sm btn-soft-success"
                                  title="Mark complete"
                                >
                                  <i className="mgc_check_line"></i>
                                </button>
                                <button
                                  onClick={() => void handleDelete(activity.id || "")}
                                  className="btn btn-sm btn-soft-danger"
                                >
                                  <i className="mgc_delete_line"></i>
                                </button>
                              </>
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

export default ActivityList;

