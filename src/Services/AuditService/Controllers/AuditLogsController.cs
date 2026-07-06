using AuditService.DTOs;
using AuditService.Services;
using HedefTakip.Shared.Constants;
using HedefTakip.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditService.Controllers;

[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService) =>
        _auditLogService = auditLogService;

    // POST /api/audit-logs  ← servisler tarafından çağrılır (X-Api-Key)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var log = await _auditLogService.CreateAsync(request, ipAddress);
        return CreatedAtAction(nameof(GetById), new { id = log.Id },
            ApiResponse<AuditLogDto>.Ok(log));
    }

    // GET /api/audit-logs  ← Admin sorgular (JWT)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Query([FromQuery] AuditLogQueryParams queryParams)
    {
        var result = await _auditLogService.QueryAsync(queryParams);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }

    // GET /api/audit-logs/{id}  ← Admin sorgular (JWT)
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var log = await _auditLogService.GetByIdAsync(id);
        if (log is null)
            return NotFound(ApiResponse.Fail(Messages.General.UnexpectedError, "Audit log bulunamadı.", "Kayıt bulunamadı."));

        return Ok(ApiResponse<AuditLogDto>.Ok(log));
    }
}
