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
        Guid payrollId)
    {
        return await service.GetByIdAsync(payrollId);
    }

    public async Task<IEnumerable<PayrollDto>> GetPayrolls(
        [Service] IPayrollService service,
        Guid? employeeId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value);
        if (startDate.HasValue && endDate.HasValue)
            return await service.GetByPeriodAsync(startDate.Value, endDate.Value);
        return await service.GetAllAsync();
    }

    // Leave Queries
    public async Task<LeaveRequestDto?> GetLeaveRequest(
        [Service] ILeaveService service,
        Guid leaveRequestId)
    {
        return await service.GetLeaveRequestByIdAsync(leaveRequestId);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequests(
        [Service] ILeaveService service,
        Guid? employeeId = null)
    {
        if (employeeId.HasValue)
            return await service.GetLeaveRequestsByEmployeeAsync(employeeId.Value);
        return await service.GetPendingLeaveRequestsAsync();
    }

    // Time Tracking Queries
    public async Task<TimeEntryDto?> GetTimeEntry(
        [Service] ITimeTrackingService service,
        Guid timeEntryId)
    {
        return await service.GetByIdAsync(timeEntryId);
    }

    public async Task<IEnumerable<TimeEntryDto>> GetTimeEntries(
        [Service] ITimeTrackingService service,
        Guid? employeeId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value);
        if (startDate.HasValue && endDate.HasValue)
            return await service.GetByDateRangeAsync(startDate.Value, endDate.Value);
        return await service.GetAllAsync();
    }

    // Performance Queries
    public async Task<PerformanceReviewDto?> GetPerformanceReview(
        [Service] IPerformanceService service,
        Guid reviewId)
    {
        return await service.GetByIdAsync(reviewId);
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetPerformanceReviews(
        [Service] IPerformanceService service,
        Guid? employeeId = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value);
        return await service.GetAllAsync();
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
        Guid trainingId)
    {
        return await service.GetByIdAsync(trainingId);
    }

    public async Task<IEnumerable<TrainingDto>> GetTrainings(
        [Service] ITrainingService service,
        Guid? employeeId = null,
        string? status = null)
    {
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status);
        return await service.GetAllAsync();
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
        return await service.GetAllAsync(tenantId, parentDepartmentId);
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

