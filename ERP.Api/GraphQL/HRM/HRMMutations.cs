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
        PayrollDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create payroll");
    }

    public async Task<PayrollDto> UpdatePayroll(
        [Service] IPayrollService service,
        PayrollDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update payroll");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Payroll not found");
    }

    public async Task<bool> DeletePayroll(
        [Service] IPayrollService service,
        Guid payrollId,
        Guid tenantId)
    {
        return await service.DeleteAsync(payrollId, tenantId);
    }

    public async Task<bool> ProcessPayroll(
        [Service] IPayrollService service,
        Guid payrollId,
        Guid tenantId,
        Guid userId)
    {
        return await service.ProcessPayrollAsync(payrollId, tenantId, userId);
    }

    // Leave Mutations
    public async Task<LeaveRequestDto> AddLeaveRequest(
        [Service] ILeaveService service,
        CreateLeaveRequestDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.AddLeaveRequestAsync(input, tenantId, userId);
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequest(
        [Service] ILeaveService service,
        UpdateLeaveRequestDto input,
        Guid tenantId)
    {
        return await service.UpdateLeaveRequestAsync(input, tenantId);
    }

    public async Task<bool> DeleteLeaveRequest(
        [Service] ILeaveService service,
        Guid leaveRequestId,
        Guid tenantId)
    {
        return await service.DeleteLeaveRequestAsync(leaveRequestId, tenantId);
    }

    public async Task<LeaveRequestDto> ApproveLeave(
        [Service] ILeaveService service,
        LeaveApprovalDto input,
        Guid tenantId,
        Guid approverId)
    {
        return await service.ApproveLeaveAsync(input, tenantId, approverId);
    }

    public async Task<LeaveRequestDto> RejectLeave(
        [Service] ILeaveService service,
        RejectLeaveRequestDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.RejectLeaveAsync(input, tenantId, userId);
    }

    public async Task<LeaveRequestDto> CancelLeave(
        [Service] ILeaveService service,
        Guid leaveRequestId,
        Guid tenantId,
        Guid userId,
        string? reason = null)
    {
        return await service.CancelLeaveAsync(leaveRequestId, reason, tenantId, userId);
    }

    // Time Tracking Mutations
    public async Task<TimeEntryDto> AddTimeEntry(
        [Service] ITimeTrackingService service,
        CreateTimeEntryDto input,
        Guid tenantId)
    {
        var id = await service.CreateAsync(input, tenantId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create time entry");
    }

    public async Task<TimeEntryDto> UpdateTimeEntry(
        [Service] ITimeTrackingService service,
        UpdateTimeEntryDto input,
        Guid tenantId)
    {
        var success = await service.UpdateAsync(input, tenantId);
        if (!success)
            throw new Exception("Failed to update time entry");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Time entry not found");
    }

    public async Task<bool> DeleteTimeEntry(
        [Service] ITimeTrackingService service,
        Guid timeEntryId,
        Guid tenantId)
    {
        return await service.DeleteAsync(timeEntryId, tenantId);
    }

    // Attendance Mutations
    public async Task<AttendanceRecordDto> CheckIn(
        [Service] IAttendanceService service,
        CheckInDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CheckInAsync(input, tenantId, userId);
    }

    public async Task<AttendanceRecordDto> CheckOut(
        [Service] IAttendanceService service,
        CheckOutDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CheckOutAsync(input, tenantId, userId);
    }

    public async Task<AttendanceRecordDto> AdjustAttendance(
        [Service] IAttendanceService service,
        AttendanceAdjustmentDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.AdjustAttendanceAsync(input, tenantId, userId);
    }

    // Performance Mutations
    public async Task<PerformanceReviewDto> AddPerformanceReview(
        [Service] IPerformanceService service,
        CreatePerformanceReviewDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create performance review");
    }

    public async Task<PerformanceReviewDto> UpdatePerformanceReview(
        [Service] IPerformanceService service,
        UpdatePerformanceReviewDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update performance review");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Performance review not found");
    }

    public async Task<bool> DeletePerformanceReview(
        [Service] IPerformanceService service,
        Guid reviewId,
        Guid tenantId)
    {
        return await service.DeleteAsync(reviewId, tenantId);
    }

    public async Task<bool> SubmitPerformanceReview(
        [Service] IPerformanceService service,
        Guid reviewId,
        Guid tenantId,
        Guid userId) =>
        await service.SubmitAsync(reviewId, tenantId, userId);

    public async Task<bool> ApprovePerformanceReview(
        [Service] IPerformanceService service,
        Guid reviewId,
        Guid tenantId,
        Guid userId) =>
        await service.ApproveAsync(reviewId, tenantId, userId);

    public async Task<bool> CompletePerformanceReview(
        [Service] IPerformanceService service,
        Guid reviewId,
        Guid tenantId,
        Guid userId) =>
        await service.CompleteAsync(reviewId, tenantId, userId);

    // Recruitment Mutations
    public async Task<JobOpeningDto> AddJobOpening(
        [Service] IRecruitmentService service,
        CreateJobOpeningDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateJobOpeningAsync(input, tenantId, userId);
        return await service.GetJobOpeningByIdAsync(id, tenantId) ?? throw new Exception("Failed to create job opening");
    }

    public async Task<JobOpeningDto> UpdateJobOpening(
        [Service] IRecruitmentService service,
        UpdateJobOpeningDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateJobOpeningAsync(input, tenantId, userId);
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

    public async Task<bool> CloseJobOpening(
        [Service] IRecruitmentService service,
        Guid jobOpeningId,
        Guid tenantId,
        Guid userId) =>
        await service.CloseJobOpeningAsync(jobOpeningId, tenantId, userId);

    public async Task<bool> FillJobOpening(
        [Service] IRecruitmentService service,
        Guid jobOpeningId,
        Guid tenantId,
        Guid userId) =>
        await service.FillJobOpeningAsync(jobOpeningId, tenantId, userId);

    public async Task<JobApplicationDto> AddJobApplication(
        [Service] IRecruitmentService service,
        CreateJobApplicationDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateJobApplicationAsync(input, tenantId, userId);
        return await service.GetJobApplicationByIdAsync(id, tenantId) ?? throw new Exception("Failed to create job application");
    }

    public async Task<bool> ShortlistJobApplication(
        [Service] IRecruitmentService service,
        Guid applicationId,
        Guid tenantId,
        Guid userId) =>
        await service.ShortlistApplicationAsync(applicationId, tenantId, userId);

    public async Task<bool> HireJobApplication(
        [Service] IRecruitmentService service,
        Guid applicationId,
        Guid tenantId,
        Guid userId) =>
        await service.HireApplicationAsync(applicationId, tenantId, userId);

    public async Task<bool> RejectJobApplication(
        [Service] IRecruitmentService service,
        Guid applicationId,
        Guid tenantId,
        Guid userId) =>
        await service.RejectApplicationAsync(applicationId, tenantId, userId);

    public async Task<CandidateDto> AddCandidate(
        [Service] IRecruitmentService service,
        CreateCandidateDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateCandidateAsync(input, tenantId, userId);
        return await service.GetCandidateByIdAsync(id, tenantId) ?? throw new Exception("Failed to create candidate");
    }

    public async Task<InterviewDto> AddInterview(
        [Service] IRecruitmentService service,
        CreateInterviewDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateInterviewAsync(input, tenantId, userId);
        return await service.GetInterviewByIdAsync(id, tenantId) ?? throw new Exception("Failed to create interview");
    }

    public async Task<bool> CompleteInterview(
        [Service] IRecruitmentService service,
        Guid interviewId,
        int rating,
        string? feedback,
        Guid tenantId,
        Guid userId) =>
        await service.CompleteInterviewAsync(interviewId, rating, feedback, tenantId, userId);

    public async Task<bool> CancelInterview(
        [Service] IRecruitmentService service,
        Guid interviewId,
        Guid tenantId,
        Guid userId) =>
        await service.CancelInterviewAsync(interviewId, tenantId, userId);

    // Training Mutations
    public async Task<TrainingDto> AddTraining(
        [Service] ITrainingService service,
        CreateTrainingDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create training");
    }

    public async Task<TrainingDto> UpdateTraining(
        [Service] ITrainingService service,
        UpdateTrainingDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update training");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Training not found");
    }

    public async Task<bool> DeleteTraining(
        [Service] ITrainingService service,
        Guid trainingId,
        Guid tenantId)
    {
        return await service.DeleteAsync(trainingId, tenantId);
    }

    public async Task<bool> EnrollEmployeeInTraining(
        [Service] ITrainingService service,
        Guid trainingId,
        Guid employeeId,
        Guid tenantId,
        Guid userId)
    {
        return await service.EnrollEmployeeAsync(trainingId, employeeId, tenantId, userId);
    }

    public async Task<bool> CompleteEmployeeTraining(
        [Service] ITrainingService service,
        Guid trainingId,
        Guid employeeId,
        decimal score,
        Guid tenantId,
        Guid userId)
    {
        return await service.CompleteTrainingAsync(trainingId, employeeId, score, tenantId, userId);
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

    public async Task<BenefitDto> UnenrollEmployeeFromBenefit(
        [Service] IBenefitsService service,
        Guid benefitId,
        UnenrollEmployeeDto input,
        Guid tenantId,
        Guid userId)
        => await service.UnenrollEmployeeAsync(benefitId, input, tenantId, userId);
}

