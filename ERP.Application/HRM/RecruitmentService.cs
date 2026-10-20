using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class RecruitmentService : IRecruitmentService
{
    private readonly IJobOpeningRepository _jobOpenings;
    private readonly IJobApplicationRepository _applications;
    private readonly ICandidateRepository _candidates;
    private readonly IInterviewRepository _interviews;

    public RecruitmentService(
        IJobOpeningRepository jobOpenings,
        IJobApplicationRepository applications,
        ICandidateRepository candidates,
        IInterviewRepository interviews)
    {
        _jobOpenings = jobOpenings;
        _applications = applications;
        _candidates = candidates;
        _interviews = interviews;
    }

    private static string? UserRef(Guid userId) => userId == Guid.Empty ? null : userId.ToString();

    private static bool Is(string? status, string expected) =>
        string.Equals(status, expected, StringComparison.OrdinalIgnoreCase);

    private static void RequireTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");
    }

    public async Task<JobOpeningDto?> GetJobOpeningByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _jobOpenings.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapOpening(entity);
    }

    public async Task<IEnumerable<JobOpeningDto>> GetAllJobOpeningsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<JobOpeningDto>();
        return (await _jobOpenings.GetAllAsync(tenantId)).Select(MapOpening);
    }

    public async Task<Guid> CreateJobOpeningAsync(CreateJobOpeningDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");

        var entity = new JobOpening
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Department = dto.Department?.Trim() ?? string.Empty,
            Location = dto.Location?.Trim() ?? string.Empty,
            EmploymentType = string.IsNullOrWhiteSpace(dto.EmploymentType) ? "FullTime" : dto.EmploymentType.Trim(),
            ExperienceLevel = dto.ExperienceLevel?.Trim() ?? string.Empty,
            SkillsRequired = dto.SkillsRequired?.Trim() ?? string.Empty,
            SalaryRange = dto.SalaryRange?.Trim() ?? string.Empty,
            Requirements = dto.Requirements,
            Responsibilities = dto.Responsibilities,
            Benefits = dto.Benefits,
            Status = "Open",
            PostedDate = (dto.PostedDate == default ? DateTime.UtcNow : dto.PostedDate).ToUniversalTime(),
            ClosingDate = dto.ClosingDate?.ToUniversalTime(),
            HiringManagerId = dto.HiringManagerId,
            PostedBy = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = UserRef(userId)
        };
        var created = await _jobOpenings.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateJobOpeningAsync(UpdateJobOpeningDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _jobOpenings.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Open"))
            throw new InvalidOperationException("Only Open job openings can be updated.");

        if (dto.Title != null) entity.Title = dto.Title.Trim();
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Department != null) entity.Department = dto.Department;
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.EmploymentType != null) entity.EmploymentType = dto.EmploymentType;
        if (dto.ExperienceLevel != null) entity.ExperienceLevel = dto.ExperienceLevel;
        if (dto.SkillsRequired != null) entity.SkillsRequired = dto.SkillsRequired;
        if (dto.SalaryRange != null) entity.SalaryRange = dto.SalaryRange;
        if (dto.Requirements != null) entity.Requirements = dto.Requirements;
        if (dto.Responsibilities != null) entity.Responsibilities = dto.Responsibilities;
        if (dto.Benefits != null) entity.Benefits = dto.Benefits;
        if (dto.PostedDate.HasValue) entity.PostedDate = dto.PostedDate.Value.ToUniversalTime();
        if (dto.ClosingDate.HasValue) entity.ClosingDate = dto.ClosingDate.Value.ToUniversalTime();
        if (dto.HiringManagerId.HasValue) entity.HiringManagerId = dto.HiringManagerId;
        entity.UpdatedBy = UserRef(userId);
        await _jobOpenings.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteJobOpeningAsync(Guid id, Guid tenantId)
    {
        var entity = await _jobOpenings.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Open"))
            throw new InvalidOperationException("Only Open job openings can be deleted.");
        return await _jobOpenings.DeleteAsync(id, tenantId);
    }

    public Task<bool> CloseJobOpeningAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionOpeningAsync(id, tenantId, userId, "Open", "Closed");

    public Task<bool> FillJobOpeningAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionOpeningAsync(id, tenantId, userId, "Open", "Filled");

    public Task<bool> ReopenJobOpeningAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionOpeningAsync(id, tenantId, userId, "Closed", "Open");

    private async Task<bool> TransitionOpeningAsync(
        Guid id, Guid tenantId, Guid userId, string from, string to)
    {
        var entity = await _jobOpenings.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, from))
            throw new InvalidOperationException($"Job opening must be {from} to move to {to}.");
        entity.Status = to;
        entity.UpdatedBy = UserRef(userId);
        await _jobOpenings.UpdateAsync(entity);
        return true;
    }

    public async Task<JobApplicationDto?> GetJobApplicationByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _applications.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapApplication(entity);
    }

    public async Task<IEnumerable<JobApplicationDto>> GetAllJobApplicationsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<JobApplicationDto>();
        return (await _applications.GetAllAsync(tenantId)).Select(MapApplication);
    }

    public async Task<Guid> CreateJobApplicationAsync(CreateJobApplicationDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (dto.JobOpeningId == Guid.Empty) throw new ArgumentException("JobOpeningId is required.");
        if (dto.CandidateId == Guid.Empty) throw new ArgumentException("CandidateId is required.");

        var opening = await _jobOpenings.GetByIdAsync(dto.JobOpeningId, tenantId)
            ?? throw new InvalidOperationException("Job opening not found.");
        if (!Is(opening.Status, "Open"))
            throw new InvalidOperationException("Can only apply to Open job openings.");

        _ = await _candidates.GetByIdAsync(dto.CandidateId, tenantId)
            ?? throw new InvalidOperationException("Candidate not found.");

        var existing = (await _applications.GetByJobOpeningAsync(dto.JobOpeningId, tenantId))
            .Any(a => a.CandidateId == dto.CandidateId);
        if (existing)
            throw new InvalidOperationException("Candidate already applied to this opening.");

        var entity = new JobApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            JobOpeningId = dto.JobOpeningId,
            CandidateId = dto.CandidateId,
            Status = "Applied",
            AppliedDate = (dto.AppliedDate == default ? DateTime.UtcNow : dto.AppliedDate).ToUniversalTime(),
            CoverLetter = dto.CoverLetter ?? string.Empty,
            ResumePath = dto.ResumePath ?? string.Empty,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = UserRef(userId)
        };
        var created = await _applications.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateJobApplicationAsync(UpdateJobApplicationDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _applications.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Applied"))
            throw new InvalidOperationException("Only Applied applications can be updated.");

        if (dto.CoverLetter != null) entity.CoverLetter = dto.CoverLetter;
        if (dto.ResumePath != null) entity.ResumePath = dto.ResumePath;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        entity.UpdatedBy = UserRef(userId);
        await _applications.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteJobApplicationAsync(Guid id, Guid tenantId)
    {
        var entity = await _applications.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Applied"))
            throw new InvalidOperationException("Only Applied applications can be deleted.");
        return await _applications.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<JobApplicationDto>> GetApplicationsByJobOpeningAsync(
        Guid jobOpeningId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<JobApplicationDto>();
        return (await _applications.GetByJobOpeningAsync(jobOpeningId, tenantId)).Select(MapApplication);
    }

    public Task<bool> ShortlistApplicationAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionApplicationAsync(id, tenantId, userId, "Applied", "Shortlisted");

    public Task<bool> MarkApplicationInterviewedAsync(Guid id, Guid tenantId, Guid userId) =>
        TransitionApplicationAsync(id, tenantId, userId, "Shortlisted", "Interviewed");

    public async Task<bool> HireApplicationAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _applications.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Interviewed") && !Is(entity.Status, "Shortlisted"))
            throw new InvalidOperationException("Application must be Shortlisted or Interviewed to hire.");

        entity.Status = "Hired";
        entity.UpdatedBy = UserRef(userId);
        await _applications.UpdateAsync(entity);

        var candidate = await _candidates.GetByIdAsync(entity.CandidateId, tenantId);
        if (candidate != null)
        {
            candidate.Status = "Hired";
            candidate.UpdatedBy = UserRef(userId);
            await _candidates.UpdateAsync(candidate);
        }

        var opening = await _jobOpenings.GetByIdAsync(entity.JobOpeningId, tenantId);
        if (opening != null && Is(opening.Status, "Open"))
        {
            opening.Status = "Filled";
            opening.UpdatedBy = UserRef(userId);
            await _jobOpenings.UpdateAsync(opening);
        }

        return true;
    }

    public async Task<bool> RejectApplicationAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _applications.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (Is(entity.Status, "Hired") || Is(entity.Status, "Rejected"))
            throw new InvalidOperationException("Application is already terminal.");

        entity.Status = "Rejected";
        entity.UpdatedBy = UserRef(userId);
        await _applications.UpdateAsync(entity);
        return true;
    }

    private async Task<bool> TransitionApplicationAsync(
        Guid id, Guid tenantId, Guid userId, string from, string to)
    {
        var entity = await _applications.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, from))
            throw new InvalidOperationException($"Application must be {from} to move to {to}.");
        entity.Status = to;
        entity.UpdatedBy = UserRef(userId);
        await _applications.UpdateAsync(entity);
        return true;
    }

    public async Task<CandidateDto?> GetCandidateByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _candidates.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapCandidate(entity);
    }

    public async Task<IEnumerable<CandidateDto>> GetAllCandidatesAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<CandidateDto>();
        return (await _candidates.GetAllAsync(tenantId)).Select(MapCandidate);
    }

    public async Task<Guid> CreateCandidateAsync(CreateCandidateDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (string.IsNullOrWhiteSpace(dto.FirstName)) throw new ArgumentException("FirstName is required.");
        if (string.IsNullOrWhiteSpace(dto.LastName)) throw new ArgumentException("LastName is required.");
        if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email is required.");

        var entity = new Candidate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
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
            Status = "Active",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = UserRef(userId)
        };
        var created = await _candidates.CreateAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateCandidateAsync(UpdateCandidateDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _candidates.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        if (dto.FirstName != null) entity.FirstName = dto.FirstName.Trim();
        if (dto.LastName != null) entity.LastName = dto.LastName.Trim();
        if (dto.Email != null) entity.Email = dto.Email.Trim();
        if (dto.Phone != null) entity.Phone = dto.Phone;
        if (dto.Address != null) entity.Address = dto.Address;
        if (dto.City != null) entity.City = dto.City;
        if (dto.State != null) entity.State = dto.State;
        if (dto.ZipCode != null) entity.ZipCode = dto.ZipCode;
        if (dto.Country != null) entity.Country = dto.Country;
        if (dto.DateOfBirth.HasValue) entity.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender != null) entity.Gender = dto.Gender;
        if (dto.Education != null) entity.Education = dto.Education;
        if (dto.Experience != null) entity.Experience = dto.Experience;
        if (dto.Skills != null) entity.Skills = dto.Skills;
        if (dto.LinkedInProfile != null) entity.LinkedInProfile = dto.LinkedInProfile;
        if (dto.PortfolioUrl != null) entity.PortfolioUrl = dto.PortfolioUrl;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        entity.UpdatedBy = UserRef(userId);
        await _candidates.UpdateAsync(entity);
        return true;
    }

    public Task<bool> DeleteCandidateAsync(Guid id, Guid tenantId) =>
        _candidates.DeleteAsync(id, tenantId);

    public async Task<InterviewDto?> GetInterviewByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _interviews.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapInterview(entity);
    }

    public async Task<IEnumerable<InterviewDto>> GetAllInterviewsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<InterviewDto>();
        return (await _interviews.GetAllAsync(tenantId)).Select(MapInterview);
    }

    public async Task<Guid> CreateInterviewAsync(CreateInterviewDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (dto.JobApplicationId == Guid.Empty) throw new ArgumentException("JobApplicationId is required.");
        if (dto.InterviewerId == Guid.Empty) throw new ArgumentException("InterviewerId is required.");

        var application = await _applications.GetByIdAsync(dto.JobApplicationId, tenantId)
            ?? throw new InvalidOperationException("Job application not found.");
        if (Is(application.Status, "Rejected") || Is(application.Status, "Hired"))
            throw new InvalidOperationException("Cannot schedule interview for a terminal application.");

        var scheduled = (dto.ScheduledDate == default
            ? (dto.InterviewDate == default ? DateTime.UtcNow : dto.InterviewDate)
            : dto.ScheduledDate).ToUniversalTime();

        var entity = new Interview
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            JobApplicationId = dto.JobApplicationId,
            CandidateId = application.CandidateId,
            InterviewerId = dto.InterviewerId,
            InterviewType = string.IsNullOrWhiteSpace(dto.InterviewType) ? "InPerson" : dto.InterviewType.Trim(),
            ScheduledDate = scheduled,
            InterviewDate = scheduled,
            Duration = dto.Duration > 0 ? dto.Duration : 60,
            Location = dto.Location,
            Notes = dto.Notes,
            Status = "Scheduled",
            Feedback = string.Empty,
            Rating = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = UserRef(userId)
        };
        return await _interviews.CreateAsync(entity);
    }

    public async Task<bool> UpdateInterviewAsync(UpdateInterviewDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _interviews.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Scheduled"))
            throw new InvalidOperationException("Only Scheduled interviews can be updated.");

        if (dto.InterviewerId.HasValue && dto.InterviewerId != Guid.Empty)
            entity.InterviewerId = dto.InterviewerId.Value;
        if (dto.InterviewType != null) entity.InterviewType = dto.InterviewType;
        if (dto.ScheduledDate.HasValue)
        {
            entity.ScheduledDate = dto.ScheduledDate.Value.ToUniversalTime();
            entity.InterviewDate = entity.ScheduledDate;
        }
        if (dto.Duration.HasValue) entity.Duration = dto.Duration.Value;
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        entity.UpdatedBy = UserRef(userId);
        return await _interviews.UpdateAsync(entity);
    }

    public async Task<bool> DeleteInterviewAsync(Guid id, Guid tenantId)
    {
        var entity = await _interviews.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Scheduled"))
            throw new InvalidOperationException("Only Scheduled interviews can be deleted.");
        return await _interviews.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<InterviewDto>> GetInterviewsByCandidateAsync(
        Guid candidateId, Guid tenantId) =>
        (await _interviews.GetByCandidateAsync(candidateId, tenantId)).Select(MapInterview);

    public async Task<bool> CompleteInterviewAsync(
        Guid id, int rating, string? feedback, Guid tenantId, Guid userId)
    {
        var entity = await _interviews.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Scheduled"))
            throw new InvalidOperationException("Only Scheduled interviews can be completed.");

        entity.Status = "Completed";
        entity.Rating = rating;
        entity.Feedback = feedback ?? string.Empty;
        entity.UpdatedBy = UserRef(userId);
        await _interviews.UpdateAsync(entity);

        var application = await _applications.GetByIdAsync(entity.JobApplicationId, tenantId);
        if (application != null && Is(application.Status, "Shortlisted"))
        {
            application.Status = "Interviewed";
            application.UpdatedBy = UserRef(userId);
            await _applications.UpdateAsync(application);
        }

        return true;
    }

    public async Task<bool> CancelInterviewAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _interviews.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!Is(entity.Status, "Scheduled"))
            throw new InvalidOperationException("Only Scheduled interviews can be cancelled.");
        entity.Status = "Cancelled";
        entity.UpdatedBy = UserRef(userId);
        return await _interviews.UpdateAsync(entity);
    }

    private static JobOpeningDto MapOpening(JobOpening e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        Title = e.Title,
        Description = e.Description,
        Department = e.Department,
        Location = e.Location,
        EmploymentType = e.EmploymentType,
        ExperienceLevel = e.ExperienceLevel,
        SkillsRequired = e.SkillsRequired,
        SalaryRange = e.SalaryRange,
        Requirements = e.Requirements,
        Responsibilities = e.Responsibilities,
        Benefits = e.Benefits,
        Status = e.Status,
        PostedDate = e.PostedDate,
        ClosingDate = e.ClosingDate,
        HiringManagerId = e.HiringManagerId,
        PostedBy = e.PostedBy,
        CreatedAt = e.CreatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedAt = e.UpdatedAt,
        UpdatedBy = e.UpdatedBy,
        IsActive = e.IsActive
    };

    private static JobApplicationDto MapApplication(JobApplication e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        JobOpeningId = e.JobOpeningId,
        CandidateId = e.CandidateId,
        Status = e.Status,
        AppliedDate = e.AppliedDate,
        CoverLetter = e.CoverLetter,
        ResumePath = e.ResumePath,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedAt = e.UpdatedAt,
        UpdatedBy = e.UpdatedBy,
        IsActive = e.IsActive
    };

    private static CandidateDto MapCandidate(Candidate e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        FirstName = e.FirstName,
        LastName = e.LastName,
        FullName = (e.FirstName + " " + e.LastName).Trim(),
        Email = e.Email,
        Phone = e.Phone,
        Address = e.Address,
        City = e.City,
        State = e.State,
        ZipCode = e.ZipCode,
        Country = e.Country,
        DateOfBirth = e.DateOfBirth,
        Gender = e.Gender,
        Education = e.Education,
        Experience = e.Experience,
        Skills = e.Skills,
        LinkedInProfile = e.LinkedInProfile,
        PortfolioUrl = e.PortfolioUrl,
        Notes = e.Notes,
        Status = e.Status,
        CreatedAt = e.CreatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedAt = e.UpdatedAt,
        UpdatedBy = e.UpdatedBy,
        IsActive = e.IsActive
    };

    private static InterviewDto MapInterview(Interview e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        JobApplicationId = e.JobApplicationId,
        CandidateId = e.CandidateId,
        InterviewerId = e.InterviewerId,
        InterviewType = e.InterviewType,
        ScheduledDate = e.ScheduledDate,
        InterviewDate = e.InterviewDate,
        Duration = e.Duration,
        Location = e.Location,
        Status = e.Status,
        Notes = e.Notes,
        Feedback = e.Feedback,
        Rating = e.Rating,
        CreatedAt = e.CreatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedAt = e.UpdatedAt,
        UpdatedBy = e.UpdatedBy,
        IsActive = e.IsActive
    };
}
