import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { performanceReviewService, type PerformanceReviewDto } from "../services";
import { PERFORMANCE_REVIEW_STATUS_OPTIONS } from "../types";

const PerformanceReviewList: React.FC = () => {
  const [reviews, setReviews] = useState<PerformanceReviewDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadReviews();
  }, [filterStatus]);

  const loadReviews = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await performanceReviewService.getAll();
      let filtered = data;
      if (filterStatus) {
        filtered = data.filter((review) => review.status === filterStatus);
      }
      setReviews(filtered);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load performance reviews");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this performance review?")) return;

    try {
      await performanceReviewService.delete(id);
      loadReviews();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete performance review");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = PERFORMANCE_REVIEW_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "Approved":
        return "bg-info";
      case "InProgress":
        return "bg-warning";
      case "Draft":
        return "bg-secondary";
      case "Rejected":
        return "bg-danger";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const getRatingStars = (rating?: number) => {
    if (!rating) return "-";
    return "★".repeat(rating) + "☆".repeat(5 - rating);
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("reviewDate");
  const sortedReviews = useMemo(
    () => sortRows(reviews),
    [reviews, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Performance Reviews" subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Performance Reviews</h4>
            <Link to="/hrm/performance-reviews/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Performance Review
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
              {PERFORMANCE_REVIEW_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="employeeName" label="Employee" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="reviewerName" label="Reviewer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="reviewPeriod" label="Review Period" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="reviewDate" label="Review Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="overallRating" label="Overall Rating" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {reviews.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center py-8">
                        No performance reviews found
                      </td>
                    </tr>
                  ) : (
                    sortedReviews.map((review) => (
                      <tr key={review.id}>
                        <td>{review.employeeName || review.employeeId}</td>
                        <td>{review.reviewerName || review.reviewerId || "-"}</td>
                        <td>
                          {review.reviewPeriodStart && review.reviewPeriodEnd
                            ? `${formatDate(review.reviewPeriodStart)} - ${formatDate(review.reviewPeriodEnd)}`
                            : "-"}
                        </td>
                        <td>{formatDate(review.reviewDate)}</td>
                        <td>
                          <span className="text-yellow-500">{getRatingStars(review.overallRating)}</span>
                          {review.overallRating && ` (${review.overallRating}/5)`}
                        </td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(review.status)}`}>
                            {getStatusLabel(review.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/hrm/performance-reviews/${review.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {review.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(review.id || "")}
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

export default PerformanceReviewList;

