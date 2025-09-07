using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces;

public interface IRecruitmentService
{
    // Job Openings
    Task<JobOpeningDto?> GetJobOpeningByIdAsync(Guid id);
    Task<IEnumerable<JobOpeningDto>> GetAllJobOpeningsAsync();
    Task<Guid> CreateJobOpeningAsync(CreateJobOpeningDto dto);
    Task<bool> UpdateJobOpeningAsync(UpdateJobOpeningDto dto);
    Task<bool> DeleteJobOpeningAsync(Guid id);
    
    // Job Applications
    Task<JobApplicationDto?> GetJobApplicationByIdAsync(Guid id);
    Task<IEnumerable<JobApplicationDto>> GetAllJobApplicationsAsync();
    Task<Guid> CreateJobApplicationAsync(CreateJobApplicationDto dto);
    Task<bool> UpdateJobApplicationAsync(UpdateJobApplicationDto dto);
    Task<bool> DeleteJobApplicationAsync(Guid id);
    Task<IEnumerable<JobApplicationDto>> GetApplicationsByJobOpeningAsync(Guid jobOpeningId);
    
    // Candidates
    Task<CandidateDto?> GetCandidateByIdAsync(Guid id);
    Task<IEnumerable<CandidateDto>> GetAllCandidatesAsync();
    Task<Guid> CreateCandidateAsync(CreateCandidateDto dto);
    Task<bool> UpdateCandidateAsync(UpdateCandidateDto dto);
    Task<bool> DeleteCandidateAsync(Guid id);
    
    // Interviews
    Task<InterviewDto?> GetInterviewByIdAsync(Guid id);
    Task<IEnumerable<InterviewDto>> GetAllInterviewsAsync();
    Task<Guid> CreateInterviewAsync(CreateInterviewDto dto);
    Task<bool> UpdateInterviewAsync(UpdateInterviewDto dto);
    Task<bool> DeleteInterviewAsync(Guid id);
    Task<IEnumerable<InterviewDto>> GetInterviewsByCandidateAsync(Guid candidateId);
}