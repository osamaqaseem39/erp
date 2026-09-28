import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { projectService, type ProjectDto } from "../services";
import { PROJECT_STATUS_OPTIONS, PROJECT_PRIORITY_OPTIONS } from "../types";

const ProjectList: React.FC = () => {
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadProjects();
  }, [filterStatus]);

  const loadProjects = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: ProjectDto[];
      if (filterStatus) {
        data = await projectService.getByStatus(filterStatus);
      } else {
        data = await projectService.getAll();
      }
      setProjects(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load projects");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this project?")) return;

    try {
      await projectService.delete(id);
      loadProjects();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete project");
    }
  };

  const handleStart = async (id: string) => {
    if (!window.confirm("Are you sure you want to start this project?")) return;

    try {
      await projectService.start(id);
      alert("Project started successfully!");
      loadProjects();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to start project");
    }
  };

  const handleComplete = async (id: string) => {
    if (!window.confirm("Are you sure you want to complete this project?")) return;

    try {
      await projectService.complete(id);
      alert("Project completed successfully!");
      loadProjects();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to complete project");
    }
  };

  const handlePause = async (id: string) => {
    const reason = window.prompt("Please enter pause reason:");
    if (!reason) return;

    try {
      await projectService.pause(id, reason);
      alert("Project paused successfully!");
      loadProjects();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to pause project");
    }
  };

  const handleCancel = async (id: string) => {
    const reason = window.prompt("Please enter cancellation reason:");
    if (!reason) return;

    try {
      await projectService.cancel(id, reason);
      alert("Project cancelled successfully!");
      loadProjects();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to cancel project");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = PROJECT_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "badge-soft-success";
      case "InProgress":
        return "badge-soft-info";
      case "Planning":
        return "badge-soft-secondary";
      case "OnHold":
        return "badge-soft-warning";
      case "Cancelled":
        return "badge-soft-danger";
      default:
        return "badge-soft-secondary";
    }
  };

  const getPriorityBadgeClass = (priority?: string) => {
    switch (priority) {
      case "Critical":
        return "badge-soft-danger";
      case "High":
        return "badge-soft-warning";
      case "Medium":
        return "badge-soft-info";
      case "Low":
        return "badge-soft-secondary";
      default:
        return "badge-soft-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("projectName");
  const sortedProjects = useMemo(
    () => sortRows(projects),
    [projects, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Projects" subName="Projects" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Projects</h4>
            <Link to="/projects/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Project
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
              {PROJECT_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="projectCode" label="Project Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="projectName" label="Project Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="clientName" label="Client" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="projectManagerName" label="Manager" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="startDate" label="Start Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="endDate" label="End Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="budget" label="Budget" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="actualCost" label="Actual Cost" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="progress" label="Progress" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="priority" label="Priority" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {projects.length === 0 ? (
                    <tr>
                      <td colSpan={12} className="text-center py-8">
                        No projects found
                      </td>
                    </tr>
                  ) : (
                    sortedProjects.map((project) => (
                      <tr key={project.id}>
                        <td>{project.projectCode}</td>
                        <td>{project.projectName}</td>
                        <td>{project.clientName || "-"}</td>
                        <td>{project.projectManagerName || "-"}</td>
                        <td>{formatDate(project.startDate)}</td>
                        <td>{formatDate(project.endDate)}</td>
                        <td>{project.budget ? project.budget.toFixed(2) : "-"}</td>
                        <td>{project.actualCost ? project.actualCost.toFixed(2) : "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <div className="w-full bg-gray-200 rounded-full h-2">
                              <div
                                className="bg-primary h-2 rounded-full"
                                style={{ width: `${project.progress || 0}%` }}
                              ></div>
                            </div>
                            <span className="text-sm">{project.progress || 0}%</span>
                          </div>
                        </td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(project.status)}`}>
                            {getStatusLabel(project.status)}
                          </span>
                        </td>
                        <td>
                          <span className={`badge ${getPriorityBadgeClass(project.priority)}`}>
                            {project.priority || "Medium"}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/projects/${project.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {project.status === "Planning" && (
                              <button
                                onClick={() => handleStart(project.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Start"
                              >
                                <i className="mgc_play_line"></i>
                              </button>
                            )}
                            {(project.status === "InProgress") && (
                              <button
                                onClick={() => handleComplete(project.id || "")}
                                className="btn btn-sm btn-soft-info"
                                title="Complete"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {(project.status === "Planning" || project.status === "InProgress") && (
                              <button
                                onClick={() => handlePause(project.id || "")}
                                className="btn btn-sm btn-soft-warning"
                                title="Pause"
                              >
                                <i className="mgc_pause_line"></i>
                              </button>
                            )}
                            {project.status !== "Completed" && project.status !== "Cancelled" && (
                              <button
                                onClick={() => handleCancel(project.id || "")}
                                className="btn btn-sm btn-soft-danger"
                                title="Cancel"
                              >
                                <i className="mgc_close_line"></i>
                              </button>
                            )}
                            {project.status === "Planning" && (
                              <button
                                onClick={() => handleDelete(project.id || "")}
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

export default ProjectList;

