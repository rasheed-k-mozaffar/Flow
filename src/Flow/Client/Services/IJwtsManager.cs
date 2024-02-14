namespace Flow.Client.Services;

public interface IJwtsManager
{
    /// <summary>
    /// Reads the JWT stored in the browser's local storage using the key from appsettings
    /// </summary>
    /// <returns></returns>
    Task<string?> GetJwtAsync();

    /// <summary>
    /// Writes the JWT to the browser's storage under the key from appsettings
    /// The destination is determined through the parameter "isPersistent"
    /// </summary>
    /// <param name="token"></param>
    /// <param name="isPersistent">If true, token gets stored in local storage, else, in session storage</param>
    /// <returns></returns>
    Task SetJwtAsync(string token, bool isPersistent);

    /// <summary>
    /// Checks to see if the local storage contains an entry for a JWT
    /// </summary>
    /// <returns>Boolean indicating whether a token was found or not</returns>
    Task<bool> CheckForJwtAsync();
}
