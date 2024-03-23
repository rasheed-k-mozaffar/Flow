using Flow.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Flow.Server.Decorators;

public class CachedUserSettingsRepository : IUserSettingsRepository
{
    private readonly UserSettingsRepository _decorated;
    private readonly IMemoryCache _cache;

    public CachedUserSettingsRepository(UserSettingsRepository decorated, IMemoryCache cache)
    {
        _decorated = decorated;
        _cache = cache;
    }

    public async Task<UserSettings> GetUserSettingsAsync(string userId)
    {
        string key = $"setting-{userId}";

#pragma warning disable CS8603 // Possible null reference return.
        return await _cache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                return _decorated.GetUserSettingsAsync(userId);
            });
#pragma warning restore CS8603 // Possible null reference return.
    }

    public async Task InsertNewSettingsEntryAsync(AppUser user, UserSettings settings)
    {
        await _decorated.InsertNewSettingsEntryAsync(user, settings);
    }

    public async Task SetColorSchemeAsync(Guid settingsEntryId, int colorSchemeId)
    {
        await _decorated.SetColorSchemeAsync(settingsEntryId, colorSchemeId);
    }

    public async Task SetThemeAsync(Guid settingsEntryId, Theme theme)
    {
        await _decorated.SetThemeAsync(settingsEntryId, theme);
    }

    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        try
        {
            await _decorated.UpdateUserSettingsAsync(settings);
        }
        catch (ResourceNotFoundException)
        {
            throw;
        }
    }
}
