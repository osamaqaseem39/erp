using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using HotChocolate;

namespace ERP.Api.GraphQL.HRM;

[ExtendObjectType("Query")]
public class HRMQueries
{
    // Payroll Queries
    public async Task<PayrollDto?> GetPayroll(
        [Service] IPayrollService service,
        Guid payrollId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(payrollId, tenantId);
    }

    public async Task<IEnumerable<PayrollDto>> GetPayrolls(
        [Service] IPayrollService service,
        Guid tenantId,
        Guid? employeeId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value, tenantId);
        if (startDate.HasValue && endDate.HasValue)
            return await service.GetByPeriodAsync(startDate.Value, endDate.Value, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Leave Queries
    public async Task<LeaveRequestDto?> GetLeaveRequest(
        [Service] ILeaveService service,
        Guid leaveRequestId,
        Guid tenantId)
    {
        return await service.GetLeaveRequestByIdAsync(leaveRequestId, tenantId);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequests(
        [Service] ILeaveService service,
        Guid tenantId,
        Guid? employeeId = null)
    {
        if (employeeId.HasValue)
            return await service.GetLeaveRequestsByEmployeeAsync(employeeId.Value, tenantId);
        return await service.GetPendingLeaveRequestsAsync(tenantId);
    }

    // Time Tracking Queries
    public async Task<TimeEntryDto?> GetTimeEntry(
        [Service] ITimeTrackingService service,
        Guid timeEntryId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(timeEntryId, tenantId);
    }

    public async Task<IEnumerable<TimeEntryDto>> GetTimeEntries(
        [Service] ITimeTrackingService service,
        Guid tenantId,
        Guid? employeeId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value, tenantId);
        if (startDate.HasValue && endDate.HasValue)
            return await service.GetByDateRangeAsync(startDate.Value, endDate.Value, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    public async Task<decimal> GetTimeEntryTotalHours(
        [Service] ITimeTrackingService service,
        Guid employeeId,
        DateTime startDate,
        DateTime endDate,
        Guid tenantId)
        => await service.GetTotalHoursAsync(employeeId, startDate, endDate, tenantId);

    // Attendance Queries
    public async Task<IEnumerable<AttendanceRecordDto>> GetEmployeeAttendance(
        [Service] IAttendanceService service,
        Guid employeeId,
        Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.Date.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow.Date;
        return await service.GetEmployeeAttendanceAsync(employeeId, tenantId, from, to);
    }

    public async Task<AttendanceSummaryDto> GetAttendanceSummary(
        [Service] IAttendanceService service,
        Guid employeeId,
        Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.Date.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow.Date;
        return await service.GetAttendanceSummaryAsync(employeeId, tenantId, from, to);
    }

    // Performance Queries
    public async Task<PerformanceReviewDto?> GetPerformanceReview(
        [Service] IPerformanceService service,
        Guid reviewId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(reviewId, tenantId);
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetPerformanceReviews(
        [Service] IPerformanceService service,
        Guid tenantId,
        Guid? employeeId = null,
        Guid? reviewerId = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value, tenantId);
        if (reviewerId.HasValue)
            return await service.GetByReviewerAsync(reviewerId.Value, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Recruitment Queries
    public async Task<JobOpeningDto?> GetJobOpening(
        [Service] IRecruitmentService service,
        Guid jobOpeningId,
        Guid tenantId)
    {
        return await service.GetJobOpeningByIdAsync(jobOpeningId, tenantId);
    }

    public async Task<IEnumerable<JobOpeningDto>> GetJobOpenings(
        [Service] IRecruitmentService service,
        Guid tenantId)
    {
        return await service.GetAllJobOpeningsAsync(tenantId);
    }

    public async Task<JobApplicationDto?> GetJobApplication(
        [Service] IRecruitmentService service,
        Guid applicationId,
        Guid tenantId)
    {
        return await service.GetJobApplicationByIdAsync(applicationId, tenantId);
    }

    public async Task<IEnumerable<JobApplicationDto>> GetJobApplications(
        [Service] IRecruitmentService service,
        Guid tenantId,
        Guid? jobOpeningId = null)
    {
        if (jobOpeningId.HasValue)
            return await service.GetApplicationsByJobOpeningAsync(jobOpeningId.Value, tenantId);
        return await service.GetAllJobApplicationsAsync(tenantId);
    }

    public async Task<CandidateDto?> GetCandidate(
        [Service] IRecruitmentService service,
        Guid candidateId,
        Guid tenantId)
    {
        return await service.GetCandidateByIdAsync(candidateId, tenantId);
    }

    public async Task<IEnumerable<CandidateDto>> GetCandidates(
        [Service] IRecruitmentService service,
        Guid tenantId)
    {
        return await service.GetAllCandidatesAsync(tenantId);
    }

    public async Task<InterviewDto?> GetInterview(
        [Service] IRecruitmentService service,
        Guid interviewId,
        Guid tenantId)
    {
        return await service.GetInterviewByIdAsync(interviewId, tenantId);
    }

    public async Task<IEnumerable<InterviewDto>> GetInterviews(
        [Service] IRecruitmentService service,
        Guid tenantId,
        Guid? candidateId = null)
    {
        if (candidateId.HasValue)
            return await service.GetInterviewsByCandidateAsync(candidateId.Value, tenantId);
        return await service.GetAllInterviewsAsync(tenantId);
    }

    // Training Queries
    public async Task<TrainingDto?> GetTraining(
        [Service] ITrainingService service,
        Guid trainingId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(trainingId, tenantId);
    }

    public async Task<IEnumerable<TrainingDto>> GetTrainings(
        [Service] ITrainingService service,
        Guid tenantId,
        Guid? employeeId = null,
        string? status = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Department Queries
    public async Task<DepartmentDto?> GetDepartment(
        [Service] IDepartmentService service,
        Guid departmentId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(departmentId, tenantId);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartments(
        [Service] IDepartmentService service,
        Guid tenantId,
        Guid? parentDepartmentId = null)
    {
        return await service.GetAllAsync(tenantId, parentDepartmentId: parentDepartmentId);
    }

    // Benefits Queries
    public async Task<BenefitDto?> GetBenefit(
        [Service] IBenefitsService service,
        Guid benefitId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(benefitId, tenantId);
    }

    public async Task<IEnumerable<BenefitDto>> GetBenefits(
        [Service] IBenefitsService service,
        Guid tenantId,
        string? category = null,
        bool? isActive = null,
        Guid? employeeId = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value, tenantId);
        if (isActive == true)
            return await service.GetActiveAsync(tenantId);
        return await service.GetAllAsync(tenantId, category, isActive);
    }
}

