using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserSettingsRepository> _logger;

    public UserSettingsRepository
    (
        AppDbContext db,
        ILogger<UserSettingsRepository> logger,
        UserInfo userInfo
    )
    {
        _db = db;
        _logger = logger;

    }

    public async Task<UserSettings> GetUserSettingsAsync(string userId)
    {
        var entry = await _db
                        .SettingsEntries
                        .FirstOrDefaultAsync(s => s.AppUserId == userId);

        if (entry is null)
            throw new ResourceNotFoundException("Resource was not found");

        return entry;
    }

    public async Task InsertNewSettingsEntryAsync(AppUser user, UserSettings settings)
    {
        settings.AppUserId = user.Id;
        settings.AppUser = user;

        var entityEntry = await _db
                                .SettingsEntries
                                .AddAsync(settings);

        if (entityEntry.State is EntityState.Added)
        {
            await _db.SaveChangesAsync();

            _logger.LogInformation
            (
                "New settings entry was inserted in the db for {name}",
                $"{user.FirstName} {user.LastName}"
            );
            return;
        }

        _logger.LogError
        (
            "Failed to insert a new settings entry row for user {name}",
            $"{user.FirstName} {user.LastName}"
        );
    }

    public async Task SetColorSchemeAsync(Guid settingsEntryId, int colorSchemeId)
    {
        var colorScheme = await _db
                                .ColorSchemes
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cs => cs.Id == colorSchemeId);

        if (colorScheme is null)
            throw new ResourceNotFoundException("Color scheme was not found");

        var settingsEntry = await _db
                                .SettingsEntries
                                .FindAsync(settingsEntryId);

        if (settingsEntry is null)
            throw new ResourceNotFoundException("Resource was not found");

        settingsEntry.ColorScheme = colorScheme;
        settingsEntry.ColorSchemeId = colorScheme.Id;

        await _db.SaveChangesAsync();
    }

    public async Task SetThemeAsync(Guid settingsEntryId, Theme theme)
    {
        var settingsEntry = await _db
                                .SettingsEntries
                                .FindAsync(settingsEntryId);

        if (settingsEntry is null)
            throw new ResourceNotFoundException("Resource was not found");

        settingsEntry.Theme = theme;
        await _db.SaveChangesAsync();
    }

    public Task UpdateUserSettingsAsync(UserSettings settings)
    {
        throw new NotImplementedException();
    }
}
