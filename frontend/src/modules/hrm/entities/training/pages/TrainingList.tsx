import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { trainingService, type TrainingDto } from "../services";
import { TRAINING_STATUS_OPTIONS, TRAINING_TYPE_OPTIONS } from "../types";

const TrainingList: React.FC = () => {
  const [trainings, setTrainings] = useState<TrainingDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadTrainings();
  }, [filterStatus]);

  const loadTrainings = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: TrainingDto[];
      if (filterStatus) {
        data = await trainingService.getByStatus(filterStatus);
      } else {
        data = await trainingService.getAll();
      }
      setTrainings(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load trainings");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this training?")) return;

    try {
      await trainingService.delete(id);
      loadTrainings();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete training");
    }
  };

  const getTypeLabel = (type?: string) => {
    const option = TRAINING_TYPE_OPTIONS.find((opt) => opt.value === type);
    return option?.label || type || "N/A";
  };

  const getStatusLabel = (status?: string) => {
    const option = TRAINING_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "InProgress":
        return "bg-info";
      case "Scheduled":
        return "bg-primary";
      case "Cancelled":
        return "bg-danger";
      case "Postponed":
        return "bg-warning";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("startDate");
  const sortedTrainings = useMemo(
    () => sortRows(trainings),
    [trainings, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Trainings" subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Trainings</h4>
            <Link to="/hrm/trainings/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Training
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
              {TRAINING_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="title" label="Title" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="trainingType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="startDate" label="Start Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="endDate" label="End Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="duration" label="Duration" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="location" label="Location" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="instructor" label="Instructor" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="cost" label="Cost" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="maxParticipants" label="Max Participants" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {trainings.length === 0 ? (
                    <tr>
                      <td colSpan={11} className="text-center py-8">
                        No trainings found
                      </td>
                    </tr>
                  ) : (
                    sortedTrainings.map((training) => (
                      <tr key={training.id}>
                        <td>{training.title}</td>
                        <td>
                          <span className="badge bg-info">{getTypeLabel(training.trainingType)}</span>
                        </td>
                        <td>{formatDate(training.startDate)}</td>
                        <td>{formatDate(training.endDate)}</td>
                        <td>{training.duration} days</td>
                        <td>{training.location}</td>
                        <td>{training.instructorName || training.instructor || "-"}</td>
                        <td>{training.cost.toFixed(2)}</td>
                        <td>{training.maxParticipants}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(training.status)}`}>
                            {getStatusLabel(training.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/hrm/trainings/${training.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {training.status === "Scheduled" && (
                              <button
                                onClick={() => handleDelete(training.id || "")}
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

export default TrainingList;

