using HedefTakip.Shared.Constants;
using HedefTakip.Shared.Models;
using HedefTakip.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParameterService.DTOs;
using ParameterService.Services;
using System.Security.Claims;

namespace ParameterService.Controllers;

[ApiController]
[Route("api/periods")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private readonly IPeriodService _periodService;
    private readonly IAuditClient _audit;

    public PeriodsController(IPeriodService periodService, IAuditClient audit)
    {
        _periodService = periodService;
        _audit = audit;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true)
    {
        var periods = await _periodService.GetAllAsync(onlyActive);
        return Ok(ApiResponse<List<PeriodDto>>.Ok(periods));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var period = await _periodService.GetByIdAsync(id);
        if (period is null)
            return NotFound(ApiResponse.Fail(Messages.Period.NotFound, Messages.Period.NotFoundUser, Messages.Period.NotFoundUser));

        return Ok(ApiResponse<PeriodDto>.Ok(period));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePeriodRequest request)
    {
        try
        {
            var period = await _periodService.CreateAsync(request);
            _audit.Send("ParameterService", "CreatePeriod", "Period", period.Id.ToString(),
                CurrentUserId, $"{period.Name} ({period.StartDate} - {period.EndDate})");
            return CreatedAtAction(nameof(GetById), new { id = period.Id },
                ApiResponse<PeriodDto>.Ok(period, Messages.Period.Created));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse.Fail(Messages.Period.AlreadyExists, Messages.Period.AlreadyExistsUser, Messages.Period.AlreadyExistsUser));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePeriodRequest request)
    {
        var period = await _periodService.UpdateAsync(id, request);
        if (period is null)
            return NotFound(ApiResponse.Fail(Messages.Period.NotFound, Messages.Period.NotFoundUser, Messages.Period.NotFoundUser));

        _audit.Send("ParameterService", "UpdatePeriod", "Period", id.ToString(),
            CurrentUserId, period.Name);
        return Ok(ApiResponse<PeriodDto>.Ok(period, Messages.Period.Updated));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _periodService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail(Messages.Period.NotFound, Messages.Period.NotFoundUser, Messages.Period.NotFoundUser));

        _audit.Send("ParameterService", "DeletePeriod", "Period", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData(Messages.Period.Deleted));
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var period = await _periodService.SetEnabledAsync(id, true);
        if (period is null)
            return NotFound(ApiResponse.Fail(Messages.Period.NotFound, Messages.Period.NotFoundUser, Messages.Period.NotFoundUser));

        _audit.Send("ParameterService", "ActivatePeriod", "Period", id.ToString(), CurrentUserId, period.Name);
        return Ok(ApiResponse<PeriodDto>.Ok(period, "Dönem aktif edildi."));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var period = await _periodService.SetEnabledAsync(id, false);
        if (period is null)
            return NotFound(ApiResponse.Fail(Messages.Period.NotFound, Messages.Period.NotFoundUser, Messages.Period.NotFoundUser));

        _audit.Send("ParameterService", "DeactivatePeriod", "Period", id.ToString(), CurrentUserId, period.Name);
        return Ok(ApiResponse<PeriodDto>.Ok(period, "Dönem pasif edildi."));
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Close(Guid id)
    {
        try
        {
            var period = await _periodService.CloseAsync(id);
            if (period is null)
                return NotFound(ApiResponse.Fail(Messages.Period.NotFound, Messages.Period.NotFoundUser, Messages.Period.NotFoundUser));

            _audit.Send("ParameterService", "ClosePeriod", "Period", id.ToString(), CurrentUserId, period.Name);
            return Ok(ApiResponse<PeriodDto>.Ok(period, Messages.Period.Closed));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse.Fail(Messages.Period.AlreadyClosed, Messages.Period.AlreadyClosedUser, Messages.Period.AlreadyClosedUser));
        }
    }
}
