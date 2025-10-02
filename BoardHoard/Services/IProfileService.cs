using BoardHoard.Data;

namespace BoardHoard.Services;

public interface IProfileService
{
    Task<ProfileViewDto?> GetMineAsync(string userId, CancellationToken ct = default);
    Task<bool> UpsertAsync(string userId, ProfileUpdateDto dto, CancellationToken ct = default);
}
