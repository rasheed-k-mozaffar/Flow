
using Microsoft.Extensions.Caching.Memory;

namespace Flow.Server.Decorators;

public class CachedColorSchemesRepository : IColorSchemesRepository
{
    private readonly ColorSchemesRepository _decorated;
    private readonly IMemoryCache _cache;

    public CachedColorSchemesRepository(ColorSchemesRepository schemesRepository, IMemoryCache cache)
    {
        _decorated = schemesRepository;
        _cache = cache;
    }

    public async Task<ColorScheme> GetSchemeAsync(int schemeId)
    {
        try
        {
            var scheme = await _decorated.GetSchemeAsync(schemeId);
            return scheme;
        }
        catch (ResourceNotFoundException)
        {
            throw;
        }
    }

    public async Task<IEnumerable<ColorScheme>> GetSchemesAsync()
    {
        string key = "color-schemes";

#pragma warning disable CS8603 // Possible null reference return.
        return await _cache.GetOrCreateAsync(
            key,
            entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromDays(1));

                return _decorated.GetSchemesAsync();
            });
#pragma warning restore CS8603 // Possible null reference return.
    }
}
