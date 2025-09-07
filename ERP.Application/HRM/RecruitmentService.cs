using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Domain.HRM;

namespace ERP.Application.HRM;

public class RecruitmentService : IRecruitmentService
{
    private readonly IJobOpeningRepository _jobOpeningRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IInterviewRepository _interviewRepository;

    public RecruitmentService(
        IJobOpeningRepository jobOpeningRepository,
        IJobApplicationRepository jobApplicationRepository,
        ICandidateRepository candidateRepository,
        IInterviewRepository interviewRepository)
    {
        _jobOpeningRepository = jobOpeningRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _candidateRepository = candidateRepository;
        _interviewRepository = interviewRepository;
    }

    // Job Openings
    public async Task<JobOpeningDto?> GetJobOpeningByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _jobOpeningRepository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<JobOpeningDto>> GetAllJobOpeningsAsync(Guid tenantId)
    {
        var entities = await _jobOpeningRepository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateJobOpeningAsync(CreateJobOpeningDto dto)
    {
        var entity = MapToEntity(dto);
        var createdEntity = await _jobOpeningRepository.CreateAsync(entity);
        return createdEntity.Id;
    }

    public async Task<bool> UpdateJobOpeningAsync(UpdateJobOpeningDto dto, Guid tenantId)
    {
        var entity = await _jobOpeningRepository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        
        // Update properties
        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Department != null) entity.Department = dto.Department;
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.EmploymentType != null) entity.EmploymentType = dto.EmploymentType;
        if (dto.ExperienceLevel != null) entity.ExperienceLevel = dto.ExperienceLevel;
        if (dto.SalaryRange != null) entity.SalaryRange = dto.SalaryRange;
        if (dto.Requirements != null) entity.Requirements = dto.Requirements;
        if (dto.Responsibilities != null) entity.Responsibilities = dto.Responsibilities;
        if (dto.Benefits != null) entity.Benefits = dto.Benefits;
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.PostedDate.HasValue) entity.PostedDate = dto.PostedDate.Value;
        if (dto.ClosingDate.HasValue) entity.ClosingDate = dto.ClosingDate.Value;
        if (dto.HiringManagerId.HasValue) entity.HiringManagerId = dto.HiringManagerId.Value;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = dto.UpdatedBy;
        
        await _jobOpeningRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteJobOpeningAsync(Guid id, Guid tenantId)
    {
        return await _jobOpeningRepository.DeleteAsync(id, tenantId);
    }

    // Job Applications
    public async Task<JobApplicationDto?> GetJobApplicationByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _jobApplicationRepository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<JobApplicationDto>> GetAllJobApplicationsAsync(Guid tenantId)
    {
        var entities = await _jobApplicationRepository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateJobApplicationAsync(CreateJobApplicationDto dto)
    {
        var entity = MapToEntity(dto);
        var createdEntity = await _jobApplicationRepository.CreateAsync(entity);
        return createdEntity.Id;
    }

    public async Task<bool> UpdateJobApplicationAsync(UpdateJobApplicationDto dto, Guid tenantId)
    {
        var entity = await _jobApplicationRepository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        
        // Update properties
        if (dto.JobOpeningId.HasValue) entity.JobOpeningId = dto.JobOpeningId.Value;
        if (dto.CandidateId.HasValue) entity.CandidateId = dto.CandidateId.Value;
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.AppliedDate.HasValue) entity.AppliedDate = dto.AppliedDate.Value;
        if (dto.CoverLetter != null) entity.CoverLetter = dto.CoverLetter;
        if (dto.ResumePath != null) entity.ResumePath = dto.ResumePath;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = dto.UpdatedBy;
        
        await _jobApplicationRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteJobApplicationAsync(Guid id, Guid tenantId)
    {
        return await _jobApplicationRepository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<JobApplicationDto>> GetApplicationsByJobOpeningAsync(Guid jobOpeningId, Guid tenantId)
    {
        var entities = await _jobApplicationRepository.GetByJobOpeningAsync(jobOpeningId, tenantId);
        return entities.Select(MapToDto);
    }

    // Candidates
    public async Task<CandidateDto?> GetCandidateByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _candidateRepository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<CandidateDto>> GetAllCandidatesAsync(Guid tenantId)
    {
        var entities = await _candidateRepository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateCandidateAsync(CreateCandidateDto dto)
    {
        var entity = MapToEntity(dto);
        var createdEntity = await _candidateRepository.CreateAsync(entity);
        return createdEntity.Id;
    }

    public async Task<bool> UpdateCandidateAsync(UpdateCandidateDto dto, Guid tenantId)
    {
        var entity = await _candidateRepository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        
        // Update properties
        if (dto.FirstName != null) entity.FirstName = dto.FirstName;
        if (dto.LastName != null) entity.LastName = dto.LastName;
        if (dto.Email != null) entity.Email = dto.Email;
        if (dto.Phone != null) entity.Phone = dto.Phone;
        if (dto.Address != null) entity.Address = dto.Address;
        if (dto.City != null) entity.City = dto.City;
        if (dto.State != null) entity.State = dto.State;
        if (dto.ZipCode != null) entity.ZipCode = dto.ZipCode;
        if (dto.Country != null) entity.Country = dto.Country;
        if (dto.DateOfBirth.HasValue) entity.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.Gender != null) entity.Gender = dto.Gender;
        if (dto.Education != null) entity.Education = dto.Education;
        if (dto.Experience != null) entity.Experience = dto.Experience;
        if (dto.Skills != null) entity.Skills = dto.Skills;
        if (dto.LinkedInProfile != null) entity.LinkedInProfile = dto.LinkedInProfile;
        if (dto.PortfolioUrl != null) entity.PortfolioUrl = dto.PortfolioUrl;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = dto.UpdatedBy;
        
        await _candidateRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteCandidateAsync(Guid id, Guid tenantId)
    {
        return await _candidateRepository.DeleteAsync(id, tenantId);
    }

    // Interviews
    public async Task<InterviewDto?> GetInterviewByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _interviewRepository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<InterviewDto>> GetAllInterviewsAsync(Guid tenantId)
    {
        var entities = await _interviewRepository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateInterviewAsync(CreateInterviewDto dto)
    {
        var entity = MapToEntity(dto);
        var createdId = await _interviewRepository.CreateAsync(entity);
        return createdId;
    }

    public async Task<bool> UpdateInterviewAsync(UpdateInterviewDto dto, Guid tenantId)
    {
        var entity = await _interviewRepository.GetByIdAsync(dto.Id);
        if (entity == null) return false;
        
        // Update properties
        if (dto.JobApplicationId.HasValue) entity.JobApplicationId = dto.JobApplicationId.Value;
        if (dto.InterviewerId.HasValue) entity.InterviewerId = dto.InterviewerId.Value;
        if (dto.InterviewType != null) entity.InterviewType = dto.InterviewType;
        if (dto.ScheduledDate.HasValue) entity.ScheduledDate = dto.ScheduledDate.Value;
        if (dto.Duration.HasValue) entity.Duration = dto.Duration.Value;
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.Feedback != null) entity.Feedback = dto.Feedback;
        if (dto.Rating.HasValue) entity.Rating = dto.Rating.Value;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = dto.UpdatedBy;
        
        return await _interviewRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteInterviewAsync(Guid id, Guid tenantId)
    {
        return await _interviewRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<InterviewDto>> GetInterviewsByCandidateAsync(Guid candidateId, Guid tenantId)
    {
        var entities = await _interviewRepository.GetByCandidateAsync(candidateId);
        return entities.Select(MapToDto);
    }

    // Mapping methods
    private static JobOpeningDto MapToDto(JobOpening entity)
    {
        return new JobOpeningDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Title = entity.Title,
            Description = entity.Description,
            Department = entity.Department,
            Location = entity.Location,
            EmploymentType = entity.EmploymentType,
            ExperienceLevel = entity.ExperienceLevel,
            SalaryRange = entity.SalaryRange,
            Requirements = entity.Requirements,
            Responsibilities = entity.Responsibilities,
            Benefits = entity.Benefits,
            Status = entity.Status,
            PostedDate = entity.PostedDate,
            ClosingDate = entity.ClosingDate,
            HiringManagerId = entity.HiringManagerId,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    private static JobOpening MapToEntity(CreateJobOpeningDto dto)
    {
        return new JobOpening
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            Title = dto.Title,
            Description = dto.Description,
            Department = dto.Department,
            Location = dto.Location,
            EmploymentType = dto.EmploymentType,
            ExperienceLevel = dto.ExperienceLevel,
            SalaryRange = dto.SalaryRange,
            Requirements = dto.Requirements,
            Responsibilities = dto.Responsibilities,
            Benefits = dto.Benefits,
            Status = dto.Status,
            PostedDate = dto.PostedDate,
            ClosingDate = dto.ClosingDate,
            HiringManagerId = dto.HiringManagerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };
    }

    private static JobApplicationDto MapToDto(JobApplication entity)
    {
        return new JobApplicationDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            JobOpeningId = entity.JobOpeningId,
            CandidateId = entity.CandidateId,
            Status = entity.Status,
            AppliedDate = entity.AppliedDate,
            CoverLetter = entity.CoverLetter,
            ResumePath = entity.ResumePath,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    private static JobApplication MapToEntity(CreateJobApplicationDto dto)
    {
        return new JobApplication
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            JobOpeningId = dto.JobOpeningId,
            CandidateId = dto.CandidateId,
            Status = dto.Status,
            AppliedDate = dto.AppliedDate,
            CoverLetter = dto.CoverLetter,
            ResumePath = dto.ResumePath,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };
    }

    private static CandidateDto MapToDto(Candidate entity)
    {
        return new CandidateDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            ZipCode = entity.ZipCode,
            Country = entity.Country,
            DateOfBirth = entity.DateOfBirth,
            Gender = entity.Gender,
            Education = entity.Education,
            Experience = entity.Experience,
            Skills = entity.Skills,
            LinkedInProfile = entity.LinkedInProfile,
            PortfolioUrl = entity.PortfolioUrl,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    private static Candidate MapToEntity(CreateCandidateDto dto)
    {
        return new Candidate
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Education = dto.Education,
            Experience = dto.Experience,
            Skills = dto.Skills,
            LinkedInProfile = dto.LinkedInProfile,
            PortfolioUrl = dto.PortfolioUrl,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };
    }

    private static InterviewDto MapToDto(Interview entity)
    {
        return new InterviewDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            JobApplicationId = entity.JobApplicationId,
            InterviewerId = entity.InterviewerId,
            InterviewType = entity.InterviewType,
            ScheduledDate = entity.ScheduledDate,
            Duration = entity.Duration,
            Location = entity.Location,
            Status = entity.Status,
            Notes = entity.Notes,
            Feedback = entity.Feedback,
            Rating = entity.Rating,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    private static Interview MapToEntity(CreateInterviewDto dto)
    {
        return new Interview
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            JobApplicationId = dto.JobApplicationId,
            InterviewerId = dto.InterviewerId,
            InterviewType = dto.InterviewType,
            ScheduledDate = dto.ScheduledDate,
            Duration = dto.Duration,
            Location = dto.Location,
            Status = dto.Status,
            Notes = dto.Notes,
            Feedback = dto.Feedback,
            Rating = dto.Rating,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };
    }
}