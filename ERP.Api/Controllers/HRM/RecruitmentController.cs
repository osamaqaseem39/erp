using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Shared.DTOs.HRM;
using FluentValidation;
using ERP.Api.Validators.HRM;
using ERP.Application.Interfaces.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/recruitment")]
[Authorize]
public class RecruitmentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IRecruitmentService _service;
    private readonly CreateJobOpeningValidator _createJobOpeningValidator;
    private readonly UpdateJobOpeningValidator _updateJobOpeningValidator;
    private readonly CreateJobApplicationValidator _createJobApplicationValidator;
    private readonly UpdateJobApplicationValidator _updateJobApplicationValidator;
    private readonly CreateCandidateValidator _createCandidateValidator;
    private readonly UpdateCandidateValidator _updateCandidateValidator;
    private readonly CreateInterviewValidator _createInterviewValidator;
    private readonly UpdateInterviewValidator _updateInterviewValidator;

    public RecruitmentController(
        IRecruitmentService service,
        CreateJobOpeningValidator createJobOpeningValidator,
        UpdateJobOpeningValidator updateJobOpeningValidator,
        CreateJobApplicationValidator createJobApplicationValidator,
        UpdateJobApplicationValidator updateJobApplicationValidator,
        CreateCandidateValidator createCandidateValidator,
        UpdateCandidateValidator updateCandidateValidator,
        CreateInterviewValidator createInterviewValidator,
        UpdateInterviewValidator updateInterviewValidator,
        ILogger<RecruitmentController> logger) : base(logger)
    {
        _service = service;
        _createJobOpeningValidator = createJobOpeningValidator;
        _updateJobOpeningValidator = updateJobOpeningValidator;
        _createJobApplicationValidator = createJobApplicationValidator;
        _updateJobApplicationValidator = updateJobApplicationValidator;
        _createCandidateValidator = createCandidateValidator;
        _updateCandidateValidator = updateCandidateValidator;
        _createInterviewValidator = createInterviewValidator;
        _updateInterviewValidator = updateInterviewValidator;
    }

    // Job Openings
    [HttpGet("job-openings/{id}")]
    public async Task<IActionResult> GetJobOpeningById(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetJobOpeningByIdAsync(id, tenantId));
    }

    [HttpGet("job-openings")]
    public async Task<IActionResult> GetAllJobOpenings()
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetAllJobOpeningsAsync(tenantId));
    }

    [HttpPost("job-openings")]
    public async Task<IActionResult> CreateJobOpening([FromBody] CreateJobOpeningDto dto)
    {
        var validationResult = await _createJobOpeningValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateJobOpeningAsync(dto));
    }

    [HttpPut("job-openings/{id}")]
    public async Task<IActionResult> UpdateJobOpening(Guid id, [FromBody] UpdateJobOpeningDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateJobOpeningValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.UpdateJobOpeningAsync(dto, tenantId));
    }

    [HttpDelete("job-openings/{id}")]
    public async Task<IActionResult> DeleteJobOpening(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.DeleteJobOpeningAsync(id, tenantId));
    }

    // Job Applications
    [HttpGet("job-applications/{id}")]
    public async Task<IActionResult> GetJobApplicationById(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetJobApplicationByIdAsync(id, tenantId));
    }

    [HttpGet("job-applications")]
    public async Task<IActionResult> GetAllJobApplications()
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetAllJobApplicationsAsync(tenantId));
    }

    [HttpGet("job-applications/job-opening/{jobOpeningId}")]
    public async Task<IActionResult> GetApplicationsByJobOpening(Guid jobOpeningId)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetApplicationsByJobOpeningAsync(jobOpeningId, tenantId));
    }

    [HttpPost("job-applications")]
    public async Task<IActionResult> CreateJobApplication([FromBody] CreateJobApplicationDto dto)
    {
        var validationResult = await _createJobApplicationValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateJobApplicationAsync(dto));
    }

    [HttpPut("job-applications/{id}")]
    public async Task<IActionResult> UpdateJobApplication(Guid id, [FromBody] UpdateJobApplicationDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateJobApplicationValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.UpdateJobApplicationAsync(dto, tenantId));
    }

    [HttpDelete("job-applications/{id}")]
    public async Task<IActionResult> DeleteJobApplication(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.DeleteJobApplicationAsync(id, tenantId));
    }

    // Candidates
    [HttpGet("candidates/{id}")]
    public async Task<IActionResult> GetCandidateById(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetCandidateByIdAsync(id, tenantId));
    }

    [HttpGet("candidates")]
    public async Task<IActionResult> GetAllCandidates()
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetAllCandidatesAsync(tenantId));
    }

    [HttpPost("candidates")]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto dto)
    {
        var validationResult = await _createCandidateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateCandidateAsync(dto));
    }

    [HttpPut("candidates/{id}")]
    public async Task<IActionResult> UpdateCandidate(Guid id, [FromBody] UpdateCandidateDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateCandidateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.UpdateCandidateAsync(dto, tenantId));
    }

    [HttpDelete("candidates/{id}")]
    public async Task<IActionResult> DeleteCandidate(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.DeleteCandidateAsync(id, tenantId));
    }

    // Interviews
    [HttpGet("interviews/{id}")]
    public async Task<IActionResult> GetInterviewById(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetInterviewByIdAsync(id, tenantId));
    }

    [HttpGet("interviews")]
    public async Task<IActionResult> GetAllInterviews()
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetAllInterviewsAsync(tenantId));
    }

    [HttpGet("interviews/candidate/{candidateId}")]
    public async Task<IActionResult> GetInterviewsByCandidate(Guid candidateId)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetInterviewsByCandidateAsync(candidateId, tenantId));
    }

    [HttpPost("interviews")]
    public async Task<IActionResult> CreateInterview([FromBody] CreateInterviewDto dto)
    {
        var validationResult = await _createInterviewValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateInterviewAsync(dto));
    }

    [HttpPut("interviews/{id}")]
    public async Task<IActionResult> UpdateInterview(Guid id, [FromBody] UpdateInterviewDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateInterviewValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.UpdateInterviewAsync(dto, tenantId));
    }

    [HttpDelete("interviews/{id}")]
    public async Task<IActionResult> DeleteInterview(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.DeleteInterviewAsync(id, tenantId));
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}