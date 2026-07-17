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
[Route("api/parameters")]
[Authorize]
public class ParametersController : ControllerBase
{
    private readonly IParameterService _parameterService;
    private readonly IAuditClient _audit;

    public ParametersController(IParameterService parameterService, IAuditClient audit)
    {
        _parameterService = parameterService;
        _audit = audit;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    // GET /api/parameters?category=System
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var parameters = await _parameterService.GetAllAsync(category);
        return Ok(ApiResponse<List<ParameterDto>>.Ok(parameters));
    }

    // GET /api/parameters/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var parameter = await _parameterService.GetByIdAsync(id);
        if (parameter is null)
            return NotFound(ApiResponse.Fail(Messages.General.ValidationError, "Parameter not found.", "Parametre bulunamadı."));

        return Ok(ApiResponse<ParameterDto>.Ok(parameter));
    }

    // GET /api/parameters/lookup/{key}
    // Diğer servisler ve frontend için: tek key ile değer sorgulama
    [HttpGet("lookup/{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var result = await _parameterService.GetByKeyAsync(key);
        if (result is null)
            return NotFound(ApiResponse.Fail(Messages.General.ValidationError, "Parameter not found.", "Parametre bulunamadı."));

        return Ok(ApiResponse<ParameterValueDto>.Ok(result));
    }

    // POST /api/parameters/lookup
    // Birden fazla key ile toplu değer sorgulama
    [HttpPost("lookup")]
    public async Task<IActionResult> GetByKeys([FromBody] List<string> keys)
    {
        var results = await _parameterService.GetByKeysAsync(keys);
        return Ok(ApiResponse<List<ParameterValueDto>>.Ok(results));
    }

    // POST /api/parameters
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateParameterRequest request)
    {
        try
        {
            var parameter = await _parameterService.CreateAsync(request);
            _audit.Send("ParameterService", "Create", "Parameter", parameter.Id.ToString(),
                CurrentUserId, $"{parameter.Key} = {parameter.Value}");
            return CreatedAtAction(nameof(GetById), new { id = parameter.Id },
                ApiResponse<ParameterDto>.Ok(parameter, "Parametre başarıyla oluşturuldu."));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse.Fail(Messages.General.ValidationError, "Key already exists.", "Bu parametre anahtarı zaten mevcut."));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateParameterRequest request)
    {
        var parameter = await _parameterService.UpdateAsync(id, request);
        if (parameter is null)
            return NotFound(ApiResponse.Fail(Messages.General.ValidationError, "Parameter not found.", "Parametre bulunamadı."));

        _audit.Send("ParameterService", "Update", "Parameter", id.ToString(),
            CurrentUserId, $"{parameter.Key} = {parameter.Value}");
        return Ok(ApiResponse<ParameterDto>.Ok(parameter, "Parametre başarıyla güncellendi."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _parameterService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail(Messages.General.ValidationError, "Parameter not found.", "Parametre bulunamadı."));

        _audit.Send("ParameterService", "Delete", "Parameter", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData("Parametre başarıyla silindi."));
    }
}
