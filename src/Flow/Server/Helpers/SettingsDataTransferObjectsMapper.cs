﻿namespace Flow.Server.Helpers;

public static class SettingsDataTransferObjectsMapper
{
    public static UserSettingsDto ToSettingsDto(this UserSettings settings)
    {
        return new()
        {
            Id = settings.Id,
            Theme = settings.Theme,
            EnableNotificationSounds = settings.EnableNotificationSounds,
            EnableSentMessageSounds = settings.EnableSentMessageSounds,
            ActivityStatus = settings.ActivityStatus,
            ColorSchemeId = settings.ColorSchemeId,
            ColorScheme = settings.ColorScheme?.ToColorSchemeDto()
        };
    }

    public static UserSettings ToUserSettings(this UserSettingsDto settings, string userId)
    {
        return new()
        {
            Id = settings.Id,
            AppUserId = userId,
            Theme = settings.Theme,
            EnableNotificationSounds = settings.EnableNotificationSounds,
            EnableSentMessageSounds = settings.EnableSentMessageSounds,
            ActivityStatus = settings.ActivityStatus,
            ColorSchemeId = settings.ColorSchemeId,
            ColorScheme = settings.ColorScheme?.ToColorScheme()
        };
    }


    public static ColorScheme ToColorScheme(this ColorSchemeDto scheme)
    {
        return new()
        {
            Id = scheme.Id,
            Name = scheme.Name,
            AccentsColor = $"{scheme.AccentsColor?.TextColor} {scheme.AccentsColor?.BgColor}",
            SelectedMessageColor = scheme.SelectedMessageColor,
            SentMsgBubbleColor = scheme.SentMsgBubbleColor,
            ReceivedMsgBubbleColor = scheme.ReceivedMsgBubbleColor
        };
    }
    public static ColorSchemeDto ToColorSchemeDto(this ColorScheme scheme)
    {
        string[]? accentsColorComponents = scheme.AccentsColor?.Split(' ');
        return new()
        {
            Id = scheme.Id,
            Name = scheme.Name,
            AccentsColor = new AccentsColor
            {
                TextColor = accentsColorComponents?[0],
                BgColor = accentsColorComponents?[1]
            },
            SelectedMessageColor = scheme.SelectedMessageColor,
            SentMsgBubbleColor = scheme.SentMsgBubbleColor,
            ReceivedMsgBubbleColor = scheme.ReceivedMsgBubbleColor
        };
    }
}
