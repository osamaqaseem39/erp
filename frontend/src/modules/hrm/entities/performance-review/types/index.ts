// PerformanceReview Entity Types
// Based on ERP.Shared.DTOs.HRM.PerformanceReviewDto

export interface PerformanceReviewDto {
  id?: string;
  tenantId?: string;
  employeeId?: string;
  reviewerId?: string;
  reviewPeriodStart?: string;
  reviewPeriodEnd?: string;
  reviewDate?: string;
  overallRating?: number;
  goalsAchieved?: string;
  goalsNotAchieved?: string;
  strengths?: string;
  areasForImprovement?: string;
  comments?: string;
  status?: string;
  isActive?: boolean;
  employeeName?: string;
  reviewerName?: string;
  companyId?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePerformanceReviewDto {
  employeeId?: string;
  reviewerId?: string;
  reviewPeriodStart?: string;
  reviewPeriodEnd?: string;
  reviewDate?: string;
  overallRating?: number;
  goalsAchieved?: string;
  goalsNotAchieved?: string;
  strengths?: string;
  areasForImprovement?: string;
  comments?: string;
  status?: string;
  companyId?: string;
}

export interface UpdatePerformanceReviewDto {
  id: string;
  employeeId?: string;
  reviewerId?: string;
  reviewPeriodStart?: string;
  reviewPeriodEnd?: string;
  reviewDate?: string;
  overallRating?: number;
  goalsAchieved?: string;
  goalsNotAchieved?: string;
  strengths?: string;
  areasForImprovement?: string;
  comments?: string;
  status?: string;
}

// Performance Review Status Options
export const PERFORMANCE_REVIEW_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "InProgress", label: "In Progress" },
  { value: "Completed", label: "Completed" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
];

// Rating Options (1-5 scale)
export const RATING_OPTIONS = [
  { value: 1, label: "1 - Poor" },
  { value: 2, label: "2 - Below Average" },
  { value: 3, label: "3 - Average" },
  { value: 4, label: "4 - Good" },
  { value: 5, label: "5 - Excellent" },
];

