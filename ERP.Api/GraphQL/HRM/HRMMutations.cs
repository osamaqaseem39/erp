using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using HotChocolate;

namespace ERP.Api.GraphQL.HRM;

[ExtendObjectType("Mutation")]
public class HRMMutations
{
    // Payroll Mutations
    public async Task<PayrollDto> AddPayroll(
        [Service] IPayrollService service,
        PayrollDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create payroll");
    }

    public async Task<PayrollDto> UpdatePayroll(
        [Service] IPayrollService service,
        PayrollDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update payroll");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Payroll not found");
    }

    public async Task<bool> DeletePayroll(
        [Service] IPayrollService service,
        Guid payrollId)
    {
        return await service.DeleteAsync(payrollId);
    }

    public async Task<bool> ProcessPayroll(
        [Service] IPayrollService service,
        Guid payrollId)
    {
        return await service.ProcessPayrollAsync(payrollId);
    }

    // Leave Mutations
    public async Task<LeaveRequestDto> AddLeaveRequest(
        [Service] ILeaveService service,
        CreateLeaveRequestDto input)
    {
        return await service.AddLeaveRequestAsync(input);
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequest(
        [Service] ILeaveService service,
        UpdateLeaveRequestDto input)
    {
        return await service.UpdateLeaveRequestAsync(input);
    }

    public async Task<bool> DeleteLeaveRequest(
        [Service] ILeaveService service,
        Guid leaveRequestId)
    {
        await service.DeleteLeaveRequestAsync(leaveRequestId);
        return true;
    }

    public async Task<LeaveRequestDto> ApproveLeave(
        [Service] ILeaveService service,
        LeaveApprovalDto input,
        Guid approverId)
    {
        return await service.ApproveLeaveAsync(input, approverId);
    }

    // Time Tracking Mutations
    public async Task<TimeEntryDto> AddTimeEntry(
        [Service] ITimeTrackingService service,
        CreateTimeEntryDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create time entry");
    }

    public async Task<TimeEntryDto> UpdateTimeEntry(
        [Service] ITimeTrackingService service,
        UpdateTimeEntryDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update time entry");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Time entry not found");
    }

    public async Task<bool> DeleteTimeEntry(
        [Service] ITimeTrackingService service,
        Guid timeEntryId)
    {
        return await service.DeleteAsync(timeEntryId);
    }

    // Performance Mutations
    public async Task<PerformanceReviewDto> AddPerformanceReview(
        [Service] IPerformanceService service,
        CreatePerformanceReviewDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create performance review");
    }

    public async Task<PerformanceReviewDto> UpdatePerformanceReview(
        [Service] IPerformanceService service,
        UpdatePerformanceReviewDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update performance review");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Performance review not found");
    }

    public async Task<bool> DeletePerformanceReview(
        [Service] IPerformanceService service,
        Guid reviewId)
    {
        return await service.DeleteAsync(reviewId);
    }

    // Recruitment Mutations
    public async Task<JobOpeningDto> AddJobOpening(
        [Service] IRecruitmentService service,
        CreateJobOpeningDto input)
    {
        var id = await service.CreateJobOpeningAsync(input);
        return await service.GetJobOpeningByIdAsync(id, input.TenantId) ?? throw new Exception("Failed to create job opening");
    }

    public async Task<JobOpeningDto> UpdateJobOpening(
        [Service] IRecruitmentService service,
        UpdateJobOpeningDto input,
        Guid tenantId)
    {
        var success = await service.UpdateJobOpeningAsync(input, tenantId);
        if (!success)
            throw new Exception("Failed to update job opening");
        return await service.GetJobOpeningByIdAsync(input.Id, tenantId) ?? throw new Exception("Job opening not found");
    }

    public async Task<bool> DeleteJobOpening(
        [Service] IRecruitmentService service,
        Guid jobOpeningId,
        Guid tenantId)
    {
        return await service.DeleteJobOpeningAsync(jobOpeningId, tenantId);
    }

    public async Task<JobApplicationDto> AddJobApplication(
        [Service] IRecruitmentService service,
        CreateJobApplicationDto input)
    {
        var id = await service.CreateJobApplicationAsync(input);
        return await service.GetJobApplicationByIdAsync(id, input.TenantId) ?? throw new Exception("Failed to create job application");
    }

    public async Task<CandidateDto> AddCandidate(
        [Service] IRecruitmentService service,
        CreateCandidateDto input)
    {
        var id = await service.CreateCandidateAsync(input);
        return await service.GetCandidateByIdAsync(id, input.TenantId) ?? throw new Exception("Failed to create candidate");
    }

    public async Task<InterviewDto> AddInterview(
        [Service] IRecruitmentService service,
        CreateInterviewDto input)
    {
        var id = await service.CreateInterviewAsync(input);
        return await service.GetInterviewByIdAsync(id, input.TenantId) ?? throw new Exception("Failed to create interview");
    }

    // Training Mutations
    public async Task<TrainingDto> AddTraining(
        [Service] ITrainingService service,
        CreateTrainingDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create training");
    }

    public async Task<TrainingDto> UpdateTraining(
        [Service] ITrainingService service,
        UpdateTrainingDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update training");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Training not found");
    }

    public async Task<bool> DeleteTraining(
        [Service] ITrainingService service,
        Guid trainingId)
    {
        return await service.DeleteAsync(trainingId);
    }

    public async Task<bool> EnrollEmployeeInTraining(
        [Service] ITrainingService service,
        Guid trainingId,
        Guid employeeId)
    {
        return await service.EnrollEmployeeAsync(trainingId, employeeId);
    }

    // Department Mutations
    public async Task<DepartmentDto> AddDepartment(
        [Service] IDepartmentService service,
        CreateDepartmentDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<DepartmentDto> UpdateDepartment(
        [Service] IDepartmentService service,
        UpdateDepartmentDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteDepartment(
        [Service] IDepartmentService service,
        Guid departmentId,
        Guid tenantId)
    {
        return await service.DeleteAsync(departmentId, tenantId);
    }

    // Benefits Mutations
    public async Task<BenefitDto> AddBenefit(
        [Service] IBenefitsService service,
        CreateBenefitDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<BenefitDto> UpdateBenefit(
        [Service] IBenefitsService service,
        BenefitDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteBenefit(
        [Service] IBenefitsService service,
        Guid benefitId,
        Guid tenantId)
    {
        return await service.DeleteAsync(benefitId, tenantId);
    }

    public async Task<BenefitDto> EnrollEmployeeInBenefit(
        [Service] IBenefitsService service,
        Guid benefitId,
        EnrollEmployeeDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.EnrollEmployeeAsync(benefitId, input, tenantId, userId);
    }
}

