using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IRecruitmentService
{
    // Job openings
    Task<JobOpeningDto?> GetJobOpeningByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobOpeningDto>> GetAllJobOpeningsAsync(Guid tenantId);
    Task<Guid> CreateJobOpeningAsync(CreateJobOpeningDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateJobOpeningAsync(UpdateJobOpeningDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteJobOpeningAsync(Guid id, Guid tenantId);
    Task<bool> CloseJobOpeningAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> FillJobOpeningAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> ReopenJobOpeningAsync(Guid id, Guid tenantId, Guid userId);

    // Applications
    Task<JobApplicationDto?> GetJobApplicationByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobApplicationDto>> GetAllJobApplicationsAsync(Guid tenantId);
    Task<Guid> CreateJobApplicationAsync(CreateJobApplicationDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateJobApplicationAsync(UpdateJobApplicationDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteJobApplicationAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobApplicationDto>> GetApplicationsByJobOpeningAsync(Guid jobOpeningId, Guid tenantId);
    Task<bool> ShortlistApplicationAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> MarkApplicationInterviewedAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> HireApplicationAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectApplicationAsync(Guid id, Guid tenantId, Guid userId);

    // Candidates
    Task<CandidateDto?> GetCandidateByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CandidateDto>> GetAllCandidatesAsync(Guid tenantId);
    Task<Guid> CreateCandidateAsync(CreateCandidateDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateCandidateAsync(UpdateCandidateDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteCandidateAsync(Guid id, Guid tenantId);

    // Interviews
    Task<InterviewDto?> GetInterviewByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<InterviewDto>> GetAllInterviewsAsync(Guid tenantId);
    Task<Guid> CreateInterviewAsync(CreateInterviewDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateInterviewAsync(UpdateInterviewDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteInterviewAsync(Guid id, Guid tenantId);
    Task<IEnumerable<InterviewDto>> GetInterviewsByCandidateAsync(Guid candidateId, Guid tenantId);
    Task<bool> CompleteInterviewAsync(Guid id, int rating, string? feedback, Guid tenantId, Guid userId);
    Task<bool> CancelInterviewAsync(Guid id, Guid tenantId, Guid userId);
}
