import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { performanceReviewService, type CreatePerformanceReviewDto, type UpdatePerformanceReviewDto, type PerformanceReviewDto } from "../services";
import { employeeService } from "../../employee/services";
import { PERFORMANCE_REVIEW_STATUS_OPTIONS, RATING_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const PerformanceReviewForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [review, setReview] = useState<PerformanceReviewDto | null>(null);
  const [employees, setEmployees] = useState<any[]>([]);

  useEffect(() => {
    loadEmployees();
    if (isEdit && id) {
      loadReview(id);
    }
  }, [isEdit, id]);

  const loadReview = async (reviewId: string) => {
    try {
      const reviewData = await performanceReviewService.getById(reviewId);
      setReview(reviewData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load performance review");
    }
  };

  const loadEmployees = async () => {
    try {
      const data = await employeeService.getAll();
      setEmployees(data);
    } catch (err: any) {
      console.error("Failed to load employees", err);
    }
  };

  const reviewSchema = yup.object({
    employeeId: yup.string().uuid("Invalid employee").required("Employee is required"),
    reviewerId: yup.string().uuid("Invalid reviewer").nullable(),
    reviewPeriodStart: yup.string(),
    reviewPeriodEnd: yup.string(),
    reviewDate: yup.string(),
    overallRating: yup.number().min(1).max(5),
    goalsAchieved: yup.string(),
    goalsNotAchieved: yup.string(),
    strengths: yup.string(),
    areasForImprovement: yup.string(),
    comments: yup.string(),
    status: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdatePerformanceReviewDto = {
          id,
          employeeId: data.employeeId,
          reviewerId: data.reviewerId || undefined,
          reviewPeriodStart: data.reviewPeriodStart || undefined,
          reviewPeriodEnd: data.reviewPeriodEnd || undefined,
          reviewDate: data.reviewDate || undefined,
          overallRating: data.overallRating ? parseFloat(data.overallRating) : undefined,
          goalsAchieved: data.goalsAchieved || undefined,
          goalsNotAchieved: data.goalsNotAchieved || undefined,
          strengths: data.strengths || undefined,
          areasForImprovement: data.areasForImprovement || undefined,
          comments: data.comments || undefined,
          status: data.status || "Draft",
        };
        await performanceReviewService.update(id, updateData);
      } else {
        const createData: CreatePerformanceReviewDto = {
          employeeId: data.employeeId,
          reviewerId: data.reviewerId || undefined,
          reviewPeriodStart: data.reviewPeriodStart || undefined,
          reviewPeriodEnd: data.reviewPeriodEnd || undefined,
          reviewDate: data.reviewDate || undefined,
          overallRating: data.overallRating ? parseFloat(data.overallRating) : undefined,
          goalsAchieved: data.goalsAchieved || undefined,
          goalsNotAchieved: data.goalsNotAchieved || undefined,
          strengths: data.strengths || undefined,
          areasForImprovement: data.areasForImprovement || undefined,
          comments: data.comments || undefined,
          status: data.status || "Draft",
        };
        await performanceReviewService.create(createData);
      }
      navigate("/hrm/performance-reviews");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save performance review");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && review
    ? {
        employeeId: review.employeeId || "",
        reviewerId: review.reviewerId || "",
        reviewPeriodStart: review.reviewPeriodStart ? review.reviewPeriodStart.split('T')[0] : "",
        reviewPeriodEnd: review.reviewPeriodEnd ? review.reviewPeriodEnd.split('T')[0] : "",
        reviewDate: review.reviewDate ? review.reviewDate.split('T')[0] : new Date().toISOString().split('T')[0],
        overallRating: review.overallRating || "",
        goalsAchieved: review.goalsAchieved || "",
        goalsNotAchieved: review.goalsNotAchieved || "",
        strengths: review.strengths || "",
        areasForImprovement: review.areasForImprovement || "",
        comments: review.comments || "",
        status: review.status || "Draft",
      }
    : {
        employeeId: "",
        reviewerId: "",
        reviewPeriodStart: "",
        reviewPeriodEnd: "",
        reviewDate: new Date().toISOString().split('T')[0],
        overallRating: "",
        goalsAchieved: "",
        goalsNotAchieved: "",
        strengths: "",
        areasForImprovement: "",
        comments: "",
        status: "Draft",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Performance Review" : "Add Performance Review"} subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/hrm/performance-reviews" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          <VerticalForm
            onSubmit={onSubmit}
            resolver={yupResolver(reviewSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Employee"
                type="select"
                name="employeeId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select employee</option>
                {employees.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {employee.employeeCode} - {employee.firstName} {employee.lastName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Reviewer"
                type="select"
                name="reviewerId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select reviewer</option>
                {employees.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {employee.employeeCode} - {employee.firstName} {employee.lastName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Review Period Start"
                type="date"
                name="reviewPeriodStart"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Review Period End"
                type="date"
                name="reviewPeriodEnd"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Review Date"
                type="date"
                name="reviewDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Overall Rating"
                type="select"
                name="overallRating"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select rating</option>
                {RATING_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {PERFORMANCE_REVIEW_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <div className="md:col-span-2">
                <FormInput
                  label="Goals Achieved"
                  type="textarea"
                  name="goalsAchieved"
                  rows={3}
                  placeholder="List goals that were achieved"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              <div className="md:col-span-2">
                <FormInput
                  label="Goals Not Achieved"
                  type="textarea"
                  name="goalsNotAchieved"
                  rows={3}
                  placeholder="List goals that were not achieved"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              <div className="md:col-span-2">
                <FormInput
                  label="Strengths"
                  type="textarea"
                  name="strengths"
                  rows={3}
                  placeholder="Employee strengths"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              <div className="md:col-span-2">
                <FormInput
                  label="Areas for Improvement"
                  type="textarea"
                  name="areasForImprovement"
                  rows={3}
                  placeholder="Areas that need improvement"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              <div className="md:col-span-2">
                <FormInput
                  label="Comments"
                  type="textarea"
                  name="comments"
                  rows={4}
                  placeholder="Additional comments"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/hrm/performance-reviews" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Performance Review"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default PerformanceReviewForm;

