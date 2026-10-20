using ERP.Api.Errors;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Api.Validators.HRM;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/recruitment")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
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

    // Job openings
    [HttpGet("job-openings/{id}")]
    [ProducesResponseType(typeof(JobOpeningDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetJobOpeningById(Guid id)
    {
        var item = await _service.GetJobOpeningByIdAsync(id, GetTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("job-openings")]
    [ProducesResponseType(typeof(IEnumerable<JobOpeningDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllJobOpenings() =>
        Ok(await _service.GetAllJobOpeningsAsync(GetTenantId()));

    [HttpPost("job-openings")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateJobOpening([FromBody] CreateJobOpeningDto dto)
    {
        var validationResult = await _createJobOpeningValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateJobOpeningAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetJobOpeningById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("job-openings/{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateJobOpening(Guid id, [FromBody] UpdateJobOpeningDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateJobOpeningValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await _service.UpdateJobOpeningAsync(dto, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Updated" })
            : NotFound();
    }

    [HttpDelete("job-openings/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteJobOpening(Guid id) =>
        await _service.DeleteJobOpeningAsync(id, GetTenantId()) ? NoContent() : NotFound();

    [HttpPost("job-openings/{id}/close")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CloseJobOpening(Guid id) =>
        await _service.CloseJobOpeningAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Closed" }) : NotFound();

    [HttpPost("job-openings/{id}/fill")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> FillJobOpening(Guid id) =>
        await _service.FillJobOpeningAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Filled" }) : NotFound();

    [HttpPost("job-openings/{id}/reopen")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ReopenJobOpening(Guid id) =>
        await _service.ReopenJobOpeningAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Reopened" }) : NotFound();

    // Applications
    [HttpGet("job-applications/{id}")]
    [ProducesResponseType(typeof(JobApplicationDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetJobApplicationById(Guid id)
    {
        var item = await _service.GetJobApplicationByIdAsync(id, GetTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("job-applications")]
    [ProducesResponseType(typeof(IEnumerable<JobApplicationDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllJobApplications() =>
        Ok(await _service.GetAllJobApplicationsAsync(GetTenantId()));

    [HttpGet("job-applications/job-opening/{jobOpeningId}")]
    [ProducesResponseType(typeof(IEnumerable<JobApplicationDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetApplicationsByJobOpening(Guid jobOpeningId) =>
        Ok(await _service.GetApplicationsByJobOpeningAsync(jobOpeningId, GetTenantId()));

    [HttpPost("job-applications")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateJobApplication([FromBody] CreateJobApplicationDto dto)
    {
        var validationResult = await _createJobApplicationValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateJobApplicationAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetJobApplicationById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("job-applications/{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateJobApplication(Guid id, [FromBody] UpdateJobApplicationDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateJobApplicationValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await _service.UpdateJobApplicationAsync(dto, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Updated" })
            : NotFound();
    }

    [HttpDelete("job-applications/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteJobApplication(Guid id) =>
        await _service.DeleteJobApplicationAsync(id, GetTenantId()) ? NoContent() : NotFound();

    [HttpPost("job-applications/{id}/shortlist")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ShortlistApplication(Guid id) =>
        await _service.ShortlistApplicationAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Shortlisted" }) : NotFound();

    [HttpPost("job-applications/{id}/interviewed")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkInterviewed(Guid id) =>
        await _service.MarkApplicationInterviewedAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Interviewed" }) : NotFound();

    [HttpPost("job-applications/{id}/hire")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> HireApplication(Guid id) =>
        await _service.HireApplicationAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Hired" }) : NotFound();

    [HttpPost("job-applications/{id}/reject")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> RejectApplication(Guid id) =>
        await _service.RejectApplicationAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Rejected" }) : NotFound();

    // Candidates
    [HttpGet("candidates/{id}")]
    [ProducesResponseType(typeof(CandidateDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCandidateById(Guid id)
    {
        var item = await _service.GetCandidateByIdAsync(id, GetTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("candidates")]
    [ProducesResponseType(typeof(IEnumerable<CandidateDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllCandidates() =>
        Ok(await _service.GetAllCandidatesAsync(GetTenantId()));

    [HttpPost("candidates")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto dto)
    {
        var validationResult = await _createCandidateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateCandidateAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetCandidateById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("candidates/{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateCandidate(Guid id, [FromBody] UpdateCandidateDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateCandidateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await _service.UpdateCandidateAsync(dto, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Updated" })
            : NotFound();
    }

    [HttpDelete("candidates/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteCandidate(Guid id) =>
        await _service.DeleteCandidateAsync(id, GetTenantId()) ? NoContent() : NotFound();

    // Interviews
    [HttpGet("interviews/{id}")]
    [ProducesResponseType(typeof(InterviewDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetInterviewById(Guid id)
    {
        var item = await _service.GetInterviewByIdAsync(id, GetTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("interviews")]
    [ProducesResponseType(typeof(IEnumerable<InterviewDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAllInterviews() =>
        Ok(await _service.GetAllInterviewsAsync(GetTenantId()));

    [HttpGet("interviews/candidate/{candidateId}")]
    [ProducesResponseType(typeof(IEnumerable<InterviewDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetInterviewsByCandidate(Guid candidateId) =>
        Ok(await _service.GetInterviewsByCandidateAsync(candidateId, GetTenantId()));

    [HttpPost("interviews")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateInterview([FromBody] CreateInterviewDto dto)
    {
        var validationResult = await _createInterviewValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        var id = await _service.CreateInterviewAsync(dto, GetTenantId(), GetUserId());
        return CreatedAtAction(nameof(GetInterviewById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("interviews/{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateInterview(Guid id, [FromBody] UpdateInterviewDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateInterviewValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await _service.UpdateInterviewAsync(dto, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Updated" })
            : NotFound();
    }

    [HttpDelete("interviews/{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> DeleteInterview(Guid id) =>
        await _service.DeleteInterviewAsync(id, GetTenantId()) ? NoContent() : NotFound();

    [HttpPost("interviews/{id}/complete")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CompleteInterview(Guid id, [FromBody] CompleteInterviewDto? dto)
    {
        var ok = await _service.CompleteInterviewAsync(
            id, dto?.Rating ?? 0, dto?.Feedback, GetTenantId(), GetUserId());
        return ok ? Ok(new MessageResponse { Message = "Completed" }) : NotFound();
    }

    [HttpPost("interviews/{id}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CancelInterview(Guid id) =>
        await _service.CancelInterviewAsync(id, GetTenantId(), GetUserId())
            ? Ok(new MessageResponse { Message = "Cancelled" }) : NotFound();

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
