namespace Flow.Server.Repositories;

public interface IColorSchemesRepository
{
    Task<IEnumerable<ColorScheme>> GetSchemesAsync();
    Task<ColorScheme> GetSchemeAsync(int schemeId);
}
