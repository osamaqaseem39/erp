using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IRecruitmentService
{
    // Job Opening methods
    Task<JobOpeningDto?> GetJobOpeningByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobOpeningDto>> GetAllJobOpeningsAsync(Guid tenantId);
    Task<Guid> CreateJobOpeningAsync(CreateJobOpeningDto dto);
    Task<bool> UpdateJobOpeningAsync(UpdateJobOpeningDto dto, Guid tenantId);
    Task<bool> DeleteJobOpeningAsync(Guid id, Guid tenantId);
    
    // Job Application methods
    Task<JobApplicationDto?> GetJobApplicationByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobApplicationDto>> GetAllJobApplicationsAsync(Guid tenantId);
    Task<Guid> CreateJobApplicationAsync(CreateJobApplicationDto dto);
    Task<bool> UpdateJobApplicationAsync(UpdateJobApplicationDto dto, Guid tenantId);
    Task<bool> DeleteJobApplicationAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobApplicationDto>> GetApplicationsByJobOpeningAsync(Guid jobOpeningId, Guid tenantId);
    
    // Candidate methods
    Task<CandidateDto?> GetCandidateByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CandidateDto>> GetAllCandidatesAsync(Guid tenantId);
    Task<Guid> CreateCandidateAsync(CreateCandidateDto dto);
    Task<bool> UpdateCandidateAsync(UpdateCandidateDto dto, Guid tenantId);
    Task<bool> DeleteCandidateAsync(Guid id, Guid tenantId);
    
    // Interview methods
    Task<InterviewDto?> GetInterviewByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<InterviewDto>> GetAllInterviewsAsync(Guid tenantId);
    Task<Guid> CreateInterviewAsync(CreateInterviewDto dto);
    Task<bool> UpdateInterviewAsync(UpdateInterviewDto dto, Guid tenantId);
    Task<bool> DeleteInterviewAsync(Guid id, Guid tenantId);
    Task<IEnumerable<InterviewDto>> GetInterviewsByCandidateAsync(Guid candidateId, Guid tenantId);
}