using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Flow.Client.Extensions;

public static class FileConverter
{
    public static IFormFile ConvertToIFromFileFromBase64ImageString(string base64ImageData)
    {
        var stream = new MemoryStream();
        var bytes = Convert.FromBase64String(base64ImageData.Trim().Split(",")[1]);

        stream.Write(bytes);
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file.png", "file-name.png");
    }
}
