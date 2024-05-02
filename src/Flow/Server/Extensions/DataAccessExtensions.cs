namespace Flow.Server.Extensions;

public static class DataAccessExtensions
{
    public static async Task<TEntity> FindAsyncAndThrowIfNull<TEntity, TException>
    (this AppDbContext db, Guid entityId, Func<string, TException> createException, CancellationToken cancellationToken = default)
    where TEntity : class, new()
    where TException : Exception
    {
        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity ID cannot be empty");
        }

        var entity = await db
                            .Set<TEntity>()
                            .FindAsync(entityId);

        return entity ?? throw createException("No resource found");
    }
}
