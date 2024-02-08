namespace Flow.Server.Helpers;

public static class ImageDataTransferObjectsMapper
{
    /// <summary>
    /// Maps an image to an image dto used to transfer back image details in the response
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static ImageDto ToImageDto(this Image image)
    {
        return new ImageDto
        {
            Id = image.Id,
            RelativeUrl = image.RelativeUrl,
            AppUserId = image.AppUserId,
            Type = image.Type
        };
    }
}
