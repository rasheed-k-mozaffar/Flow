
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Flow.Server.Repositories;

public class ColorSchemesRepository : IColorSchemesRepository
{
    private readonly AppDbContext _db;

    public ColorSchemesRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ColorScheme> GetSchemeAsync(int schemeId)
    {
        var scheme = await _db
                            .ColorSchemes
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == schemeId);

        if (scheme is null)
            throw new ResourceNotFoundException("Scheme was not found");

        return scheme;
    }

    public async Task<IEnumerable<ColorScheme>> GetSchemesAsync()
    {
        var schemes = await _db
                            .ColorSchemes
                            .ToListAsync();

        return schemes;
    }
}
