using BoardHoard.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardHoard.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ProfileService(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<ProfileViewDto?> GetMineAsync(string userId, CancellationToken ct = default)
    {
        var profile = await _db.UserProfiles.AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return new ProfileViewDto { UserId = userId };

        return new ProfileViewDto
        {
            UserId = userId,
            DisplayName = profile.DisplayName,
            PhotoUrl = profile.PhotoUrl,
            Age = profile.Age,
            FavoriteGame = profile.FavoriteGame,
            FavoriteMechanic = profile.FavoriteMechanic,
            Bio = profile.Bio,
            TopGamesJson = profile.TopGamesJson
        };
    }

    public async Task<bool> UpsertAsync(string userId, ProfileUpdateDto dto, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return false;

        var profile = await _db.UserProfiles
            .SingleOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
        {
            profile = new UserProfile { UserId = userId };
            _db.UserProfiles.Add(profile);
        }

        profile.DisplayName = dto.DisplayName ?? profile.DisplayName;
        profile.PhotoUrl = dto.PhotoUrl;
        profile.Age = dto.Age;
        profile.FavoriteGame = dto.FavoriteGame;
        profile.FavoriteMechanic = dto.FavoriteMechanic;
        profile.Bio = dto.Bio;
        profile.TopGamesJson = dto.TopGamesJson;

        // Optionally mirror DisplayName/PhotoUrl to AspNetUsers
        if (!string.IsNullOrWhiteSpace(dto.DisplayName))
            user.DisplayName = dto.DisplayName;
        if (!string.IsNullOrWhiteSpace(dto.PhotoUrl))
            user.PhotoUrl = dto.PhotoUrl;

        await _users.UpdateAsync(user);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
