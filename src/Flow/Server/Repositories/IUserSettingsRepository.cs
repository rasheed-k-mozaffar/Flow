using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public interface IUserSettingsRepository
{
    Task InsertNewSettingsEntryAsync(AppUser user, UserSettings settings);
    Task<UserSettings> GetUserSettingsAsync(string userId);
    Task SetThemeAsync(Guid settingsEntryId, Theme theme);
    Task SetColorSchemeAsync(Guid settingsEntryId, int colorSchemeId);
    Task UpdateUserSettingsAsync(UserSettings settings);
}
