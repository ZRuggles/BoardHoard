using BoardHoard.Data;
using BoardHoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace BoardHoard.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _svc;
    private readonly UserManager<ApplicationUser> _users;

    public ProfileController(IProfileService svc, UserManager<ApplicationUser> users)
    {
        _svc = svc;
        _users = users;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileViewDto>> Me(CancellationToken ct)
    {
        var userId = _users.GetUserId(User)!;
        var dto = await _svc.GetMineAsync(userId, ct) ?? new ProfileViewDto { UserId = userId };
        return Ok(dto);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] ProfileUpdateDto dto, CancellationToken ct)
    {
        var userId = _users.GetUserId(User)!;
        var ok = await _svc.UpsertAsync(userId, dto, ct);
        return ok ? Ok() : BadRequest();
    }
}
